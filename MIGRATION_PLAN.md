# Migration plan: Pango on Linux, Parso as the hardware gateway

Companion to `CLAUDE.md`. That file records facts about the code; this one records the decisions and the task list. Only T1 and B6 are implemented so far; the ticked boxes in section 5 show progress.

## 1. Decisions

1. Pango stays a Windows Forms app and runs under Mono on Linux for now. A native rewrite (for example Avalonia) is a later, separate effort. A Mono spike (A3) gates the UI-related work.
2. On Linux all hardware goes through Parso: Picob, card reader, printer. In that mode Pango opens no serial port and no printer. Its only traffic is the Pango cloud API, the local RestApi and Parso over TCP on localhost.
3. Windows keeps working. New behavior in Pango sits behind `HardwareMode=Direct|Parso` (default `Direct`). New behavior in Parso sits behind opt-in settings whose defaults reproduce today's behavior, because other apps use Parso in production. Parso changes are additive only.
4. The QR scanner stays a keyboard wedge (`ReaderSource=Output`) handled by the app, not by Parso.
5. AMP card terminals are out of scope for now. Parso mode supports the Globalcom Retail Protocol reader only; sites with `PaymentProcessor=AMP` stay on `Direct`.
6. Picob: Parso keeps the port open, polls `C` for presence and serves the cached value. In Parso mode Pango never talks to the Picob directly.
7. The kiosk hardware is a POS58 thermal printer (58 mm, ESC/POS), a CP210x USB-serial Picob, a Globalcom card reader and a keyboard-wedge QR scanner.

## 2. Target architecture (Linux)

```
Pango (Mono, WinForms)  --TCP 127.0.0.1:1994-->  Parso (net8)  -->  Picob   (serial, kept open, C polled)
  Entry / Exit app                                                -->  card reader (Globalcom RP)
  RestApi (hosting TBD)   --------------------->                  -->  POS58 via CUPS raw queue (ESC/POS)
  QR scanner: keyboard wedge straight into the app
```

Picob firmware facts that shape the design (from the hardware engineer): the firmware only answers commands (no stream); `C` returns `{"C": 0|1}` (1 = vehicle present); `A` opens the barrier and returns `{"A": 1}`; `H` cuts power to everything including the PC, uncleanly, and never replies; `N` does not exist; at least 800 ms between any two commands (also `C` followed by `A`); asserting DTR/RTS resets the board; avoid opening the port often. Full list in `CLAUDE.md`.

## 3. Wire contract additions (Parso)

Existing keys `"1"`, `"2"` and `"3"` keep their behavior. With `Picob:PersistentConnection=FALSE` (default) nothing below applies to key `"2"`.

| Request | Reply |
|---|---|
| `{"2":"STATUS"}` | `Response` whose `DeviceResponse` is `{"connected":true,"ageMs":240,"C":0}`; `ageMs` is the time since the last successful `C`, and `C` is omitted until the first success |
| `{"2":"A"}` | `Response` with the firmware reply as `DeviceResponse` (`{"A": 1}`) |
| `{"2":"H"}` and any letter in `Picob:FireAndForgetCommands` | `Response` success without `DeviceResponse`; the command is written and not waited for |
| `{"2":"<other command>"}` | firmware reply passed through, as today |
| any `{"2":...}` while the Picob is disconnected | `Response(true, 500, false, "ERROR: PICOB NOT CONNECTED")` |
| `{"1":{"B":{"Header":"..","TicketId":"..","Date":"..","Footer":"..","QR":"<text>"}}}` | `Response` success; the `QR` variable is rendered as an image where the template has `@@QR@@` (variable ids are fixed in C3) |

Framing is unchanged: 4-byte big-endian length plus UTF-8 text. Pango (.NET Framework) must encode and decode UTF-8 explicitly because its `Encoding.Default` is not UTF-8.

## 4. Conventions for the tasks

- Sizes: S is a few dozen lines in one or two files, M is a new class or a change spread over one file.
- Owner is Claude unless marked User (needs hardware, a Windows build or a decision).
- Nothing compiles or runs in the cloud environment. After each code task the user builds (`dotnet build Parso-main/Parso/Parso.sln` for Parso, `msbuild` on Windows for Pango) and reports errors. Claude re-reads its own diff before committing.
- One task per commit, titled with the task id.
- New Parso settings are optional with defaults. Existing keys are read with a bare `int.Parse` and a missing one aborts startup; new ones must not.
- Each task adds the config keys it introduces to the config files it needs, with values that keep Windows behavior unchanged.
- Code follows the surrounding style and comment density. No notes or files inside the project folders.
- Progress: tick the task's checkbox below in the same commit that completes it, and add a dated line to the decision log (section 8) for any result or decision that later tasks rely on.

## 5. Tasks

### Phase T: tooling

- [x] **T1. Parso test client (S).** A stdlib-only Python 3 script in a `tools/` folder in the repo root (not inside either project) that sends one length-prefixed UTF-8 JSON request to Parso (host and port as arguments), prints the reply, and can keep listening for broadcast frames such as the card-inserted message. It lets each Parso task be checked from a terminal against the `TestingMode` simulators or real hardware before any Pango change exists. Depends on: none.

### Phase A: prerequisites (User)

- [ ] **A1. Confirm the shipped DLLs match the zip sources.** Open `Parso/Controllers/Picob/PicobController.dll` and `Controllers/Printer/TREAPrinting.dll` in ILSpy or dotPeek. Check for `OpenPicobPort(string, int, bool, bool, string)`, `SendToPicob(string)`, `IPrinterController.PrintDynamic(List<PrintElement>, int, string, int)` and `Print(..., printerName)`. If any is missing or differs, build the DLLs from `PicobIntegration-main.zip` / `TREAPrinting-master.zip` (net8.0) and replace them. Blocks B3 and C2.
- [ ] **A2. Capture real card-payment replies.** Full `Response` JSON from Parso for an RP payment that is approved, declined, times out and is cancelled. Blocks D1.
- [ ] **A3. Mono spike.** On the Linux kiosk image, build and run Entry under Mono (with a Roslyn `csc`) using `UseFakePos=true` and `ReaderSource=Output`. Expect fatal log lines for the missing Picob and a terminal that stays in `StandBy`. Report whether forms render (`RoundedButton`, fonts), the wedge input reaches the app, LiteDB opens and logs are written. Blocks phases F and G, not phases B to E.

### Phase B: Parso Picob (persistent port, poller, cache)

- [ ] **B1. Config plumbing (S).** Files: `appsettings.json`, `Utils/ProjectConstants.cs`, `Program.cs`. Optional keys read with defaults: `AppSettings:ListenAddress` (`0.0.0.0`), `Picob:PersistentConnection` (`FALSE`), `Picob:DtrEnable` and `Picob:RtsEnable` (`TRUE`, today's behavior), `Picob:MinCommandIntervalMs` (800, never below 800 when persistent), `Picob:PollIntervalMs` (1000), `Picob:DisconnectAfterFailures` (3), `Picob:FireAndForgetCommands` (`H`). Depends on: none. Done when Parso starts both with the unchanged `appsettings.json` and with the new keys present.
- [ ] **B2. Listen address (S).** `Classes/ServerListener.cs` binds `ListenAddress` instead of `new IPEndPoint(0, 1994)`; an invalid value logs a warning and falls back to all interfaces. Depends on: B1.
- [ ] **B3. `PicobSession` (M).** New `Classes/Helpers/PicobSession.cs`: a process-wide singleton (`Program.cs` builds a second `CommandProcessor`, so a per-helper port would be opened twice). It owns the `PicobController`, opens the port once with the configured DTR/RTS, and exposes `Send(command, waitForReply)` as the only write path. One lock covers all access, and a gate sleeps the remainder of `MinCommandIntervalMs` since the last write, so `A` right after `C` waits. Not wired in yet. Depends on: B1, A1.
- [ ] **B4. Poller, cache and reconnect (M).** In `PicobSession`: a background thread sends `C` every `PollIntervalMs` (never faster than the gate, and skipping its turn while a command is waiting), parses `{"C":n}` and stores the value and its timestamp. After `DisconnectAfterFailures` consecutive failures it marks the session disconnected, closes the port and retries opening with backoff through the same gate. `GetStatus()` returns connected, age and `C`. Depends on: B3.
- [ ] **B5. Route key "2" (M).** `Classes/Helpers/PicobHelper.cs`, `Program.cs`. With `PersistentConnection=TRUE`: start the session at startup and skip the automatic Picob clock set (the committed `AutoSetTimeEnabled=TRUE` sends a JSON object that the current firmware does not understand); implement the reply rules in section 3. With the flag off, the current code path is untouched. Depends on: B4.
- [x] **B6. Test-mode simulator (S).** `Classes/Helpers/Testing/`. With `EnablePicobTest=TRUE`, both testers answer like the current firmware: `STATUS` returns the shape in section 3, `C` toggles presence (so a developer can simulate a car), `A` returns `{"A": 1}` and `H` returns success. Neither `A` nor `H` may call `Environment.Exit` (today `A` and `K` in `ParsoTester` and `H` in `A1Tester` do). Depends on: none (the contract is fixed).

### Phase C: Parso printing (QR on ESC/POS)

- [ ] **C1. Printing config and safety (S).** `appsettings.json`, `ProjectConstants.cs`, `Program.cs`, `PrinterHelper.cs`. Optional `Printing:PaperSizeMm` (58) and `Printing:PrinterName` (empty = CUPS default queue), passed to `Print` by named arguments. A template folder that is not a rooted path resolves against `AppContext.BaseDirectory`. `printCommand` is serialized with a lock, because `PrinterHelper` keeps request state in fields and both the kiosk and the RestApi will print. Depends on: B1 (same files).
- [ ] **C2. QR rendering (M).** `Parso.csproj` (add `QRCoder` 1.6.0) and `PrinterHelper.cs`. When the rendered template contains `@@QR@@` and the request has the reserved variable `QR`, build the element list: text before the marker, a PNG QR image from `PngByteQRCode`, text after. Call `PrintDynamic(elements, paperSize, printerName)`. Text is left-aligned so the padding spaces Pango already puts in its header lines survive; the image is centered. Otherwise the existing `Print` call is unchanged. Optional `Printing:QrPixelsPerModule` (6) and `Printing:QrEccLevel` (`L`). Depends on: C1, A1.
- [ ] **C3. Templates (S).** `PrintingTemplates/`, `PrintingConfig.xml`, `Parso.csproj` (one `Content` entry per file). Template `B` is the entry ticket (header, ticket id, date, `@@QR@@`, footer); template `C` is the exit receipt (header, title, numbers, amounts, date, footer, optional `@@QR@@`). Register the variable ids so they mirror the strings Pango builds today. Depends on: C2.

### Phase D: Parso payment

- [ ] **D1. Reply mapping (S to M).** Using the A2 samples, list what Pango needs to fill `GeneratePaymentResponse` (approved flag, `TransactionStatus` Approved/Denied/Error, client transaction number, the `Ref` value that Pango uses as the ticket id, `Err code`, receipt XML, UI error text). Extend Parso's reply additively where something is missing. Depends on: A2.

### Phase E: Pango as a Parso client

- [ ] **E1. `ParsoClient` project (M).** New class library `AA.Pango.ParsoClient` (net48, old-style csproj and `packages.config` like its siblings, Newtonsoft.Json 13.0.1), added to `AA.Pango.App.sln` and referenced from `AA.Pango.ServiceLayer`. `ParsoClient`: TCP, length-prefixed UTF-8 frames, one in-flight request per instance, timeouts, lazy connect and reconnect, and non-JSON frames (the card-inserted broadcast) skipped and raised as an event. Use one instance per role (Picob, printer, payment) so out-of-order replies cannot mix. A `ParsoResponse` DTO mirrors Parso's `Response`. Settings `ParsoHost` (`127.0.0.1`) and `ParsoPort` (`1994`). This task has the most csproj risk, so the user builds right after it. Depends on: none.
- [ ] **E2. `HardwareMode` and `ParsoPicob` (S).** A small settings class reads `HardwareMode`. `ParsoPicob`: `GetStatus()`, `OpenBarrier()` (success only if the reply has `A` equal to 1) and `HardReset()`. Depends on: E1.
- [ ] **E3. Barrier and denied signal (M).** `TerminalController`. In Parso mode skip `InitPicob` in the constructor; `OpenBarrier()` calls `ParsoPicob.OpenBarrier()` and keeps today's failure path (mail, optional restart); the three live `SendTicketDeniedOnlineSignal` writes (six more sit in commented-out code) go through one private method that does nothing in Parso mode and the same write in Direct mode. Depends on: E2.
- [ ] **E4. Presence from Parso (M).** `TerminalController`. Extract the state update from `picob_OnSerialDataReceived` (the `IsVehiclePresent` / `StandBy` / `VehiclePresent` logic and the `PaymentInProgress` skip) into one method that takes a bool. The serial handler still parses ids 1 and 2 and calls it. In Parso mode a thread started from `Start()` reads `STATUS` every `PicobStatusIntervalMs` (500) and calls it with `C == 1`. While Parso reports disconnected the last state is kept and the condition is logged. Depends on: E3.
- [ ] **E5. `ParsoPaymentManager` (M).** In the new project: a `PaymentManagerAbstract` that sends `{"3":{"amountInPennies":n,"clientTransactionID":"...","testEth":false}}`, drives `CurrentReaderStatus` (`WaitingForCard` on send, `CardInserted` on the broadcast, `Idle` at the end) so `FormV4ShowReceipt` keeps its prompts, maps the reply per D1 and never throws. `TerminalController.PaymentProcessorService` gets a third branch for `PaymentProcessor=Parso` with a `PaymentProcessorService-Parso` type name. Depends on: E1, D1.
- [ ] **E6. Ticket printing (M).** `PangoService.PrintTicketQR`. In Parso mode skip both the `PostPrintTicket` HTTP branch and `PrintDocument`; build the same header, ticket id, date and footer text as today and send template `B` with `QR`. Keep the `updateTransient` / `openBarrier` handling after the print. Depends on: E1, C3.
- [ ] **E7. Receipt printing (M).** `TerminalController.PrintInvoice`, same approach with template `C`, including the optional QR. The receipt logo is not supported yet. Depends on: E1, C3.
- [ ] **E8. RestApi through Parso (M).** `HardResetService.HardReset` sends `H` through `ParsoClient` instead of opening `PicovComPort`; `RestController.PrintTicket` sends template `B`. Needs `HardwareMode` and the `Parso*` keys in `Web.config` and a reference to the new project. Keep the close-app-then-reset order in `PostHardReset`. Depends on: E1, B5, C3.

### Phase F: Linux portability in Pango (after A3)

- [ ] **F1. `AttachConsole` (S).** Both `Program.cs`: call it only on Windows and survive a missing entry point.
- [ ] **F2. Script launching (S).** `RestarProcess`, `RebootTerminal` and `CloseApp` are started with `Process.Start(path)`. Add one helper that runs the configured command through `/bin/sh -c` on Linux. Sites: `TerminalController`, `GlobalComPaymentManager`, `RestController`.
- [ ] **F3. Paths (S).** Audit backslash literals and config values: LiteDB file, `logs\`, logo path. Use `Path.Combine` where the code builds paths.
- [ ] **F4. Rendering (M).** Scope after A3: fonts (Calibri, Microsoft Sans Serif), `RoundedButton` under libgdiplus, whatever the spike reports.
- [ ] **F5. LiteDB shared mode (User).** Test the named-mutex behavior with two processes on Linux.

### Phase G: RestApi on Linux (after A3)

- [ ] **G1. Contract inventory (S).** Record every endpoint (route, verb, parameters, body, response) and the LiteDB flags it writes, so any hosting choice keeps the contract. Callers do not matter, only that the endpoints stay reachable as they are.
- [ ] **G2. Hosting decision (User and Claude).** Options: OWIN self-host of the existing Web API 2 controllers under Mono (MVC views and Swagger dropped), or an `HttpListener` module inside the kiosk process.
- [ ] **G3. Implementation.** Split into tasks once G2 is decided. Peer updates (`TerminalsToUpdate`, `PutTicket`) keep their ports.

### Phase H: deployment

- [ ] **H1. Device and printer setup notes (S).** udev rules for the CP210x adapters keyed on USB port path rather than serial number (cheap adapters share one), a raw CUPS queue for the POS58, and the X keyboard layout so the wedge scanner's Base64 payload (`+`, `/`, `=`) arrives intact.
- [ ] **H2. Parso Linux profile (S).** A template `appsettings` with persistent Picob on, DTR/RTS off, automatic clock set off, `ListenAddress=127.0.0.1`, `/dev/serial/by-path/...` device paths, the printer queue name and a relative template folder. Depends on: B5, C1.
- [ ] **H3. systemd units (S).** One for Parso, one for Pango under Mono, ordered so Parso starts first. Depends on: H2, F1 to F3.
- [ ] **H4. Kiosk acceptance (User).** Checklist run on real hardware: presence, barrier, hard reset, card payment, ticket and receipt printing, unplug and replug of the Picob.

### Later

AMP as a new `CardReaderType` in Parso (the adapter DLL is an AnyCPU managed assembly, so loading it in net8 looks feasible but is unproven); receipt logo printing; GlobalCom `-002` recovery in Parso mode; wiring `OutOfService`; QR scanner in Parso (not planned); the native Avalonia UI.

## 6. Dependencies and order

```
A1 -> B3, C2          A2 -> D1          A3 -> F*, G*
B1 -> B2, B3, C1      B3 -> B4 -> B5
C1 -> C2 -> C3        D1 -> E5
E1 -> E2 -> E3 -> E4  E1 + D1 -> E5
E1 + C3 -> E6, E7     E1 + B5 + C3 -> E8
B5 + C1 -> H2         H2 + F1..F3 -> H3
```

T1, B1, B6 and E1 have no dependencies and can start immediately. T1 is optional for everything else but makes B5, B6 and C2 much easier to verify. B6 is done, so presence, `A`, `H` and `STATUS` can be tried against the simulators. B1, C1 and B5 edit the same files (`Program.cs`, `ProjectConstants.cs`), so keep them sequential.

## 7. Open decisions and risks

- **Disconnected Picob.** E4 keeps the last state. Going out of service instead is possible, but `TerminalState.OutOfService` is only rendered by the show-forms, not the main forms.
- **GlobalCom `-002` recovery** (device reset, open barrier, mail) is not covered in Parso mode.
- **Barrier latency.** The 800 ms gate means `A` can wait up to 800 ms behind a poll. Presence can lag by about one poll interval plus the exchange time (up to roughly 1.1 s at the default).
- **`H` is not clean.** It removes power from the PC, so LiteDB and log files are at risk. Keep the close-app-then-reset order; a filesystem `sync` before `H` is an option.
- **Parso exposure.** It has no authentication and `H` cuts power, so `ListenAddress` should be loopback on the kiosks.
- **Unverified.** Everything here is untested on a compiler or hardware. A1 may show that the shipped DLLs differ from the zip sources.
- **Mixed fleet.** Windows sites with the older streaming Picob firmware keep using `Direct` mode, so Direct-mode code paths must stay behaviorally unchanged.
- **Startup order.** Pango can start before Parso or after a Parso restart. `ParsoClient` reconnects lazily (E1), but what the UI shows while Parso is unreachable is undecided; E4 only keeps the last state.

Facts still needed from the kiosks:

- Is the card reader also a CP210x USB-serial adapter? If so, `/dev/serial/by-id` can collide and H1 must key the udev rules on the USB port path.
- The CUPS queue name for the POS58, and whether `lp` is installed on the kiosk image.
- The exact Parso and Pango start order and restart policy (H3).
- How many sites run the older streaming firmware versus the polled one.

## 8. Decision log

Append one dated line per result or decision that later tasks depend on (A1 to A3 outcomes, answers to the open decisions above, hardware captures).

- 2026-10-01: Direction agreed (sections 1 and 2). Picob firmware facts recorded in `CLAUDE.md`. Detection lag of up to about 1.1 s accepted. AMP deferred. QR scanner stays a wedge.
- 2026-10-01: T1 done (`tools/parso_client.py`, run with `python3 tools/parso_client.py '{"2":"C"}'`, `--listen [seconds]` keeps reading frames). Checked only against a stub server, not against Parso. Parso's `DeviceResponse` is a string holding the device's JSON, and the card-inserted broadcast is the bare text `CARDINSERTED` (the `RPCardInsertedMessage` setting), not JSON.
- 2026-10-01: The Picob simulators in Parso do not follow the engineer's table (`A1Tester` lacks `C` and `A`; `ParsoTester` answers `C` with an array; `A`, `K` and `H` call `Environment.Exit`). B6 should precede B5, and the T1 client is only useful for Picob checks against the simulators once B6 is done.
- 2026-10-01: B6 done. In test mode both testers answer `A` with `{"A":1}`, `H` with a success `Response` without `DeviceResponse`, `STATUS` with `{"connected":true,"ageMs":100,"C":n}`, and `C` with `{"C":n}`, flipping `n` on every `C` (so only a developer should send `C`; Pango reads presence through `STATUS`). `A1Tester` state gained a `C` field in `PicobResponse`. `K` in `ParsoTester` no longer exits either. Not simulated: the disconnected reply (`PICOB NOT CONNECTED`) and a changing `ageMs`. Not built or run yet.

## 9. Picking this up in a new chat

- Read `CLAUDE.md` first, then this file. Take the first unticked task whose dependencies (section 6) are all ticked.
- The two root zips are source archives for `PicobController` and `TREAPrinting`. Extract them into the scratchpad, never into the repo.
- Nothing builds in the cloud environment. Finish each task by telling the user which build to run (section 4) and wait for the result before starting a dependent task.
- Work on the branch the session names, one task per commit, task id first in the message, no model name in commit messages.
- Questions for the hardware engineer are written in Spanish. The Picob answers so far are in `CLAUDE.md`.
