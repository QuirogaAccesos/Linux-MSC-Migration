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

Pango (built with `msbuild`, see `CLAUDE.md`) talks to Parso when its `App.config` has `HardwareMode` `LinuxController`, with `LinuxControllerHost` and `LinuxControllerPort` pointing at Parso (the keys are in both `App.config` files). Today that covers opening the barrier and vehicle presence (`PicobStatusInterval`). In Pango's log look for `Picob status from Linux Controller OK`, and for the conditions `LINUX CONTROLLER NOT AVAILABLE`, `PICOB NOT CONNECTED TO LINUX CONTROLLER` and `LINUX CONTROLLER COULD NOT OPEN THE BARRIER!!`. Start Parso with a connected Picob (section 4) first.

## 8. When you are done

Press Ctrl+C in window 1, then restore the files you edited, so none of the local config gets committed:

```powershell
cd C:\Users\TREA\Documents\GitHub\Linux-MSC-Migration
git restore Parso-main/Parso/Parso/appsettings.json
git status
```

Add any other file you touched to the `git restore` line. Report the results so they go into the decision log of `MIGRATION_PLAN.md`.
