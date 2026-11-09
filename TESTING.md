# Testing the migration work

How to run and check the Parso side from Windows PowerShell on the machine that has the hardware. Companion to `CLAUDE.md` (facts) and `MIGRATION_PLAN.md` (tasks). Nothing can be compiled or run in the cloud environment, so every check here is done by the user, and the results go into the decision log of the plan.

## 1. What you need

- .NET 8 SDK (`dotnet --list-sdks` shows a line starting with `8.`), and `git`, on the machine with the Picob, printer or card reader. The first build needs internet for the NuGet restore.
- Two PowerShell windows: window 1 builds and runs Parso and stays open, window 2 sends requests.
- Optional: Python 3 (`py`) for `tools/parso_client.py`. The PowerShell helpers below do the same without it.

## 2. Get the code and set the local config

```powershell
cd C:\Users\TREA\Documents\GitHub\Linux-MSC-Migration
git fetch origin
git checkout <branch to test>
git pull origin <branch to test>
```

If `checkout` complains about local changes, run `git stash` first and `git stash pop` at the end.

Edit the source file, never the copy in `bin`, because every `dotnet build` and `dotnet run` overwrites that copy:

```powershell
notepad Parso-main\Parso\Parso\appsettings.json
```

The settings that matter, by test. The file is JSON, so a backslash in a path must be doubled (`C:\\Users\\...`) or written as `/`; an unescaped `\U` stops Parso from starting.

| Test | Settings (under `AppSettings`) |
|---|---|
| Any | `Picob:AutoSetTimeEnabled` `FALSE` (so no clock command is sent to the Picob), `TestingMode:*` `FALSE` unless you want a simulator |
| Simulators, no hardware | `TestingMode:EnablePicobTest` `TRUE` (`CustomProcessor` `FALSE` gives `A1Tester`) |
| Real Picob | `Picob:Windows` the Picob's COM port (Device Manager, Ports), `Picob:PersistentConnection` `TRUE`, `Picob:DtrEnable` and `Picob:RtsEnable` `FALSE` (asserting them resets the board) |
| Printing | `PrintingTemplatesAbsoluteLocation` `PrintingTemplates` (relative, resolved against the app folder), `Printing:PrinterName` empty for the default printer |
| Card payments | `CardReader:Windows` the reader's COM port, `CardReader:CardReaderType` `1`, `CardReader:IncludeRPlog` `TRUE`, `CardReader:SendRPCardInsertedNotification` `TRUE` |

Run Parso (window 1) and leave it open:

```powershell
cd Parso-main\Parso
dotnet run --project Parso
```

Wait for `Server started and listening on 0.0.0.0:1994`. The log is the newest `Parso-main\Parso\Parso\bin\Debug\net8.0\logs\log-*.txt`:

```powershell
Get-Content Parso-main\Parso\Parso\bin\Debug\net8.0\logs\log-*.txt -Tail 40
```

To run a hand-edited copy in `bin` instead, use `dotnet run --project Parso --no-build`.

## 3. Send requests (window 2)

Load the helpers once per window:

```powershell
cd C:\Users\TREA\Documents\GitHub\Linux-MSC-Migration
. .\tools\parso_client.ps1
```

`Send-Parso '<json>'` prints the first frame that comes back. `Send-ParsoPayment '<json>'` prints every frame with its time until the reply arrives, which a card payment needs because the `CARDINSERTED` broadcast comes first. Add `| Tee-Object -FilePath name.txt` to keep a copy, with a different name each time.

The Python client does the same (`py tools\parso_client.py '<json>'`, `--listen` to keep reading frames), but Windows PowerShell 5.1 strips the inner quotes of an argument, so the request has to be written `'{\"2\":\"C\"}'`.

Replies are one `Response` object: `Success`, `HTTPStatus`, `OperationResult`, `Message`, `DeviceResponse`. Judge a request by `OperationResult`, not by `Success`: Parso answers its own failures with `Success` true.

## 4. Picob

Simulators (`EnablePicobTest` `TRUE`, no Picob needed):

```powershell
Send-Parso '{"2":"STATUS"}'   # {"connected":true,"ageMs":100,"C":0}
Send-Parso '{"2":"C"}'        # flips the simulated presence: {"C":1}, then {"C":0}
Send-Parso '{"2":"A"}'        # {"A":1}; Parso keeps running
Send-Parso '{"2":"H"}'        # success without a device response; Parso keeps running
```

Real Picob (`PersistentConnection` `TRUE`):

1. Picob unplugged: `STATUS` and `C` reply 500 `ERROR: PICOB NOT CONNECTED`, and the log shows one open failure, not one every few seconds.
2. Plug it in with the COM port from the config: within about 10 seconds `STATUS` shows `"connected":true`, then `ageMs` and `C` after the first poll.
3. `C` and `STATUS` follow the loop detector: 1 with a vehicle, 0 without.
4. `A` opens the barrier and replies `{"A": 1}`. It moves the real barrier.
5. Unplug it: after about three failed polls `STATUS` is `NOT CONNECTED` again, and it reconnects when you plug it back in.
6. Do not send `H` on a real kiosk: it cuts power to everything, the PC included.

## 5. Printing

Template `A` is the test ticket. Templates `B` (entry ticket) and `C` (exit receipt) have a QR marker, and the reserved variable `QR` is the text of the QR code. Send every variable of the template, empty if unused, or its `#Name#` text prints.

```powershell
Send-Parso '{"1":{"A":{"TO":"1500","ST":"1400","DES":"100","NOM":"Test"}}}'
Send-Parso '{"1":{"B":{"Header":"Line 1","TicketId":"12345","Date":"Oct 02, 2026 03:45 PM","Footer":"Scan the QR at exit","QR":"https://example.com/ticket/12345"}}}'
Send-Parso '{"1":{"C":{"Header":"Line 1","Title":"RECEIPT","ReceiptNumber":"Receipt #: 987","AuthNumber":"Auth Number: 55","Amounts":"Amount: $12.50","Date":"Date/Time: Oct 02, 2026 03:50 PM","Footer":"Support 1 (877) 697-2646","QR":"https://example.com/app"}}}'
```

Each reply should be `SUCCESS: PRINTING COMMAND SENT SUCCESSFULLY`. Check that the QR sits between the date and the footer, that nothing is cut off, and that a phone reads the QR. Without `QR` in the request the ticket prints as before, with no QR. To try another printer, set `Printing:PrinterName` to a name from `Get-Printer | Select Name` and restart Parso.

## 6. Card payments

The amount is `amountInPennies`. Against the staging gateway any amount is approved, even with an expired card, except the special amounts: 5.00 (`500`) and 6.00 (`600`) are declined, and 48.03 to 48.06 (`4803` to `4806`) simulate host errors. Use a different `clientTransactionID` and file name each time.

```powershell
Send-ParsoPayment '{"3":{"amountInPennies":30,"clientTransactionID":"T-APPROVED","testEth":false}}' | Tee-Object -FilePath approved.txt
Send-ParsoPayment '{"3":{"amountInPennies":500,"clientTransactionID":"T-DECLINED","testEth":false}}' | Tee-Object -FilePath declined.txt
Send-ParsoPayment '{"3":{"amountInPennies":4803,"clientTransactionID":"T-HOSTDOWN","testEth":false}}' | Tee-Object -FilePath hostdown.txt
```

How to tell the replies apart (the full mapping is in `MIGRATION_PLAN.md`, section 3, `Card payment replies`):

- Approved: `OperationResult` true, `SUCCESS: TRANSACTION SUCCESSFUL AND COMPLETE`, `LastTransactionStatus` `Status outcome good`.
- Declined or host error: `OperationResult` false, `...BUT INCOMPLETE`, `Status outcome bad`, and the receipt XML `Error` (`-001` host decline or host unreachable, `-002` no internet at the terminal).
- Timeout: remove the card, start a payment and wait 30 seconds. It replies 500 `ERROR: TIMEOUT`.
- Cancel: remove the card, start a payment, and within 30 seconds send `Send-Parso '{"3":{"cancelPayment":1}}'` from a third window. The cancel replies `CANCELLATION REQUEST SENT` and the payment replies `CANCELLED: PAYMENT CANCELLED BY REQUEST`. A cancel sent after the card was detected can race with the payment and does not stop it.
- Reader port held by another program: `ExceptionEncountered` true with the exception text in `Log`.

Real cards are charged on a live gateway. Mask the cardholder name and the card number before pasting a reply anywhere, and never copy the terminal's gateway key out of its support info.

## 7. Pango against Parso

Pango (built with `msbuild`, see `CLAUDE.md`) talks to Parso when its `App.config` has `HardwareMode` `LinuxController`, with `LinuxControllerHost` and `LinuxControllerPort` pointing at Parso (the keys are in both `App.config` files). Today that covers opening the barrier, vehicle presence (`PicobStatusInterval`), and printing the entry ticket and the exit receipt. In Pango's log look for `Picob status from Linux Controller OK`, and for the conditions `LINUX CONTROLLER NOT AVAILABLE`, `PICOB NOT CONNECTED TO LINUX CONTROLLER` and `LINUX CONTROLLER COULD NOT OPEN THE BARRIER!!`. Start Parso with a connected Picob (section 4) first.

Entry ticket (Entry app, `HardwareMode` `LinuxController`, Parso set up for printing as in section 5): pull a ticket at the Entry screen. Pango sends template `B` itself, even with `PostPrintTicket` `true`, and Parso's log shows the `{"1":{"B":{...}}}` request. The ticket should show the `ticketEntry.dat` header, `Ticket ID`, `Date`, the QR and the footer, and a phone should read the QR as the ticket text. Pango's log shows `Ticket printed successfully`, or `PRINTER ERROR - Linux Controller could not print the ticket` when Parso did not print it (check Parso's log for the reason).

Exit receipt (Exit app, `HardwareMode` `LinuxController`): tap the receipt button on the payment screen, then after an approved payment (and `PrintReceiptTimeout` ms) Pango sends template `C`. The receipt should show the `ReceiptHeader*` lines, the title, `Receipt #` and `Auth Number`, the amount lines, the date, the QR of `TicketQrText` when `QrPrint` is `true`, and the `ReceiptFooter*` lines. The business logo is not printed in this mode. Pango's log shows `Invoice #<n> printed successfully.` or `PRINTER ERROR - Linux Controller could not print the invoice`. With `UseFakePos` `true` the payment is approved without a card, which is enough to test the receipt.

RestApi (IIS, its `Web.config` with `HardwareMode` `LinuxController`): `POST <RestApiUrl>PrintTicket?terminalId=<id>&transientId=<n>` prints template `B` through Parso and answers `Success` true, or false with `Linux Controller could not print the ticket`. `POST <RestApiUrl>PostHardReset?installationId=<id>&terminalId=<id>` closes the kiosk app (`CloseApp`) and two seconds later sends `H` through Parso. On a real kiosk that cuts power to everything, the PC included, so try it only with the Picob simulator (`EnablePicobTest` `TRUE`), where Parso answers success and nothing happens; the RestApi log then shows `HardReset -- Successfully Hard Reset through Linux Controller`.

Card payments (Exit only) go through Parso when the Exit `App.config` has `PaymentProcessor` `LinuxController`; this is independent of `HardwareMode`. Leave `UseFakePos` `false`, keep `PaymentServiceEnabled` `true`, and set up Parso for card payments as in section 6. The Exit screen should show the card prompt while the reader waits, switch to the card-inserted prompt when Parso broadcasts `CARDINSERTED`, and then the approved, denied or failed screen. Pango's log (logger `LinuxControllerPaymentManager`) shows `Payment response: <message> - LastTransactionStatus: <status>` for each reply. Expected results with the staging amounts from section 6: 0.30 approved; 5.00 and 48.03 denied (`Error Code: -001`); no card for 30 seconds failed with `Timeout reading card`. If Parso is not running, the payment fails with `NO RESPONSE FROM LINUX CONTROLLER` once `PaymentProcessor-LinuxController-MaxPurchaseTimeout` (120 s) has passed, or at once when the connection is refused, and Pango sends a cancel if no card was inserted.

## 8. When you are done

Press Ctrl+C in window 1, then restore the files you edited, so none of the local config gets committed:

```powershell
cd C:\Users\TREA\Documents\GitHub\Linux-MSC-Migration
git restore Parso-main/Parso/Parso/appsettings.json
git status
```

Add any other file you touched to the `git restore` line. Report the results so they go into the decision log of `MIGRATION_PLAN.md`.

## 9. Mono spike (task A3)

Goal: see how far the Entry app gets on the Linux kiosk image under Mono, before any work on phases F and G. It needs task F1 (`AttachConsole` only on Windows): without it Entry stops on its first line under Mono, before any window opens. Build from a `main` that has F1.

1. On Windows, build Debug as usual (`CLAUDE.md`, Build), pack the output with `tar -czf "$env:USERPROFILE\Desktop\entry-debug.tar.gz" -C AA.Pango.App\bin\Debug .` (not a Windows zip, which can store backslashes in paths), copy it to the Linux machine (AnyDesk file transfer worked) and unpack it with `mkdir -p ~/lg1/entry && tar -xzf ~/Downloads/entry-debug.tar.gz -C ~/lg1/entry`. It holds `AA.Pango.App.exe`, the DLLs, `AA.Pango.App.exe.config` and the `.dat` / `.xml` files the app loads by relative name. Running the Windows build answers the questions of the spike; building on Linux is step 6 and optional.

2. On Linux, install Mono, the drawing library and fonts (Debian / Ubuntu names; other distributions have the same packages under similar names):

   ```bash
   sudo apt install mono-complete libgdiplus ca-certificates-mono fonts-crosextra-carlito fonts-dejavu
   mono --version
   cat /etc/os-release
   ```

   Carlito has the same metrics as Calibri, which the forms use; `Microsoft Sans Serif` falls back to the default sans font. `ca-certificates-mono` lets Mono trust the HTTPS certificate of the Pango cloud.

3. Edit `~/lg1/entry/AA.Pango.App.exe.config` (the copy, never the repo file):

   - `connectionStrings`, `LiteDB`: `Filename=/home/<user>/lg1/entry/pangodb.db;Connection=shared;` (the committed value is a Windows path).
   - `log4net`, file appender: `<file value="logs/" />` (the committed `logs\` would become a file name with a backslash).
   - Both with `sed`, from `~/lg1/entry`: `sed -i 's#Filename=[^;]*pangodb.db#Filename='"$HOME"'/lg1/entry/pangodb.db#' AA.Pango.App.exe.config` and `sed -i 's#<file value="logs\\" />#<file value="logs/" />#' AA.Pango.App.exe.config`.
   - Set `HardwareMode` to `LinuxController` and keep `ReaderSource` `Output`: in `Direct` mode Mono throws on `SerialPort.ReceivedBytesThreshold` and the app never finishes starting (A3). Set `CheckLoopDetector` to `false` unless Parso and a Picob are running, or the Entry buttons only show the vehicle-not-present panel. Entry has no `UseFakePos`; it only matters for Exit.

4. Run it from its folder, because it loads `styles.dat`, `en.dat`, `es.dat` and `languagesSupported.xml` by relative name:

   ```bash
   cd ~/lg1/entry
   mono --debug AA.Pango.App.exe 2>&1 | tee spike-run1.txt
   ```

   Close the app with Ctrl+C in the terminal if the window has no way out; if it is wedged (a dialog crashed), use `pkill -9 -f AA.Pango.App.exe` from another terminal. Move the previous log away (`mv logs/<date>.log logs/run1.log`) before a new run so each run has its own file. Stack traces may lack line numbers: Mono does not read the Windows PDB files of a Visual Studio Debug build.

5. What to report (send `spike-run1.txt`, the newest file in `~/lg1/entry/logs/`, and screenshots):

   - Mono version and distribution (step 2).
   - Did a window open, full screen, with the `V4_Ticket_PH` layout? If not, the first exception in the terminal or the log.
   - Buttons (`RoundedButton`): shape, colors, text; fonts and text that is cut off or overlaps; images.
   - Touch or mouse clicks on the buttons and the on-screen keyboards.
   - Wedge input: with the main screen in front, scan a QR, or type some text and press Enter. Pango's log should show `Got value from output: <text>` and then `***** processQRCode *****`.
   - LiteDB: `pangodb.db` appears at the configured path and the log has no LiteDB errors.
   - Logs: `logs/` exists and gets a `dd.MM.yyyy.log` file.
   - Anything else that throws, with the exception text.

   Optional second run with `HardwareMode` `LinuxController` and Parso running on the same machine (sections 2 and 4, Linux port keys): this is the target setup, it uses no serial port in Pango, and Pango's log should show `Picob status from Linux Controller OK`. Pulling a ticket in `Direct` mode posts to the RestApi (`PostPrintTicket` `true`), which does not exist on Linux, so test printing only in `LinuxController` mode.

6. Optional, building on Linux. It needs `msbuild`, which comes with the `mono-complete` package from the Mono project's own repository (not always with the distribution's package). From the repo root, after `git clone` or `git pull` on the Linux machine:

   ```bash
   cd PangoAppQR-develop
   msbuild AA.Pango.App/AA.Pango.App.csproj -t:Restore -p:RestorePackagesConfig=true
   msbuild AA.Pango.App/AA.Pango.App.csproj /p:Configuration=Debug
   ```

   Build only the Entry project: the RestApi needs IIS build targets and the Exit project needs the AMP DLL path. Report the errors if it fails; this has not been tried.

The results go into the decision log of `MIGRATION_PLAN.md`. The first run (2026-11-09) is recorded there; known problems until F4, F6 and F7 are done: the window stays at 1600x900, the on-screen keyboard lags, and the plate dialog wedges the app when its 10 s keyboard timeout runs out.
