# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Ground rules for this repo

- The repo root holds a single folder, `PangoAppQR-develop/`, which is a copy of an existing solution. The user will hand it back to its original repos, so it has to stay indistinguishable from hand-written work.
- Never create notes, docs, markdown, scratch files or tooling inside `PangoAppQR-develop/`. Anything generated for reference (this file included) lives in the repo root, next to that folder.
- Keep code changes in the style of the surrounding code. Match its comment density and language (comments are a mix of Spanish and English), and add no explanatory headers, banners or generated-looking comments.
- `MSCGitHub.lnk` inside the project is a Windows shortcut to an external `MSCGitHub` checkout. That second repo is not part of this one.
- Goal of this repo ("Linux-MSC-Migration"): port the Windows-only kiosk software to run on Linux. The sections below list what blocks that.

## Build, run, test

No `dotnet`, `mono` or `msbuild` is installed in the cloud environment, so nothing can be compiled or run there. The solution is Visual Studio 2022 and targets classic .NET Framework 4.8 (old-style csproj, `packages.config`, NuGet `HintPath`s into a `..\packages\` folder that is not committed).

- Solution: `PangoAppQR-develop/AA.Pango.App.sln`. On Windows: `nuget restore AA.Pango.App.sln`, then `msbuild AA.Pango.App.sln /p:Configuration=Debug`.
- Build a single project with `msbuild AA.Pango.App.Exit\AA.Pango.App.Exit.csproj`.
- There are no tests and no lint config anywhere in the repo. `AA.Pango.TestApp` is a console stub that echoes stdin, and `AA.Pango.TestWinApp` is a UI library despite its name (see below).
- Entry and Exit both build to `AA.Pango.App.exe`. Do not build them into the same output folder.
- `AA.PangoApp.Payments.AMP` references `AMPSerialAdapter.dll` from `..\..\..\AMP\LibAMP1.2.0\`, a path outside the repo. A copy of the DLL is in `libs/`. `AA.Pango.App.Exit` references the AMP project's output as a loose DLL at `..\AA.PangoApp.Payments.AMP\bin\Debug\...` (not a `ProjectReference`), so build AMP first, in Debug.
- Both apps read `App.config` from the exe folder and load `styles.dat`, `en.dat`, `es.dat`, `languagesSupported.xml` by relative file name, so the working directory must be the exe folder.
- `.gitignore` is inherited from an unrelated `AA.PMS` project and does not cover this solution's `bin/`, `obj/` or `packages/`.

## Solution layout

Ten projects; the dependency direction is `App / App.Exit -> UI.Utils, ServiceLayer -> Model, Payments.*`.

| Folder | Role |
|---|---|
| `AA.Pango.App` | Entry terminal, WinForms (`WinExe`) |
| `AA.Pango.App.Exit` | Exit terminal, WinForms. Adds payments and receipts. `AssemblyName` is also `AA.Pango.App` |
| `AA.Pango.TestWinApp` | Builds as `AA.Pango.App.UI.Utils.csproj`: shared UI library (on-screen keyboards, `RoundedButton`, `DataFileParser`, `LanguageLocalizationParser`) |
| `AA.Pango.ServiceLayer` | All business logic: `TerminalController`, `PangoService` (cloud client), LiteDB services |
| `AA.Pango.Model` | LiteDB entity POCOs |
| `AA.Pango.RestApi` | ASP.NET Web API 2 / MVC 5 site, hosted in IIS. Local command endpoint for the kiosks |
| `AA.PangoApp.Payments.Interface` | `IPaymentManager`, `PaymentManagerAbstract`, DTOs, `Notifications/MailSender` |
| `AA.PangoApp.Payments.GlobalCom` / `.AMP` | Two card-terminal drivers over serial |
| `AA.Pango.TestApp` | Unused console stub |
| `libs/` | Proprietary binaries `GlobalcomRetailProtocol.dll` and `AMPSerialAdapter.dll` (managed, x86, no P/Invoke) |

## Architecture

### The big picture

A kiosk (one per lane) runs either the Entry or the Exit WinForms app. The UI is thin: nearly all behavior is in the static singleton `ServiceLayer/Controllers/TerminalController.cs` (about 2,200 lines), which owns the hardware, the state machine and the background threads. The forms only poll it and render. The Pango cloud (`PangoUrl`) is the source of truth for tickets and prices, and the local LiteDB file is a cache plus an inter-process mailbox.

### Terminal state machine

`TerminalController.TerminalState` (enum at the top of the file) is a set of volatile fields that the UI polls every 200 ms. It is not an event bus. Transitions come from three sources: the Picob serial handler (vehicle present / standby), `processQRCode` (ticket states, then a 3 s thread resets to `StandBy`), and the payment thread (`PaymentInProgress` -> `Approved|Denied|Failed` -> `Finalized`). Reading the QR code flows: local `tickets` lookup, else online `checkQRcode`, with result codes mapped to states (2 TicketEntry, 3 NotFound, 4 PermitExpired, 5 PangoPaymentCVV, -2 PangoPassError). Anti-passback is a second online check when a ticket's `HasBeenUsed` disagrees with the lane direction.

### Hardware I/O (all `System.IO.Ports.SerialPort`)

- Barcode/QR reader: `ReaderSource` is `Serial` (COM port) or `Output`. `Output` is a keyboard wedge: the reader types into a focused hidden TextBox, form `KeyPress` handlers append to `controller.InputValue`, and an 800 ms thread polls it. Current configs use `Output`.
- Picob/Picov barrier controller: single-byte commands from `Commands/InputCommands.cs` (`A` = open barrier, `N` = ticket denied online, `H` = hard reset). Input lines carry loop-sensor values (ids 1 and 2 mean vehicle present).
- Card terminals (Exit only), selected by the `PaymentProcessor` setting. `PaymentProcessorService-*` hold type-name strings that are resolved with `Type.GetType` + `Activator.CreateInstance` (the AMP type is not project-referenced from ServiceLayer):
  - GlobalCom: CCI Retail Protocol over RS232, one new `RetailProtocol` per transaction, synchronous polling with `Thread.Sleep`. Error `-002` triggers `ResetDevice` (command 0x27), opens the barrier and sends a mail.
  - AMP: JSON over serial through `AMPSerialAdapter`. The port stays open for the process lifetime. Approval is `response_resultcode_key == "0000"` and `response_code_key` in `{00, 10}`. Result flags are shared between threads without synchronization.
- `UseFakePos=true` makes payments approve without hardware. Use it for any non-hardware run.

### Data and inter-process communication

- LiteDB 5 with `Connection=shared` (named mutex), opened per call with `using (new LiteDatabase(...))`. Collections: `tickets` (the whitelist), `terminals`, `payments`, `paymentAttempts`, `templates`, `templateChangeScreen`, plus legacy `applications` and `parkings`.
- The kiosk app and the IIS-hosted RestApi are separate processes that share this one file. The RestApi writes rows (`ForceOpenBarrier` / `ForceWhitelistUpload` flags on `terminals`, `paymentAttempts` from `ChargePlate`, `templateChangeScreen` from `ChangeScreen`), and the kiosk polls and consumes them (`ApiTerminalInterval`, `PendingChangeScreenInterval`, `PendingPaymentsInterval`). Moving off LiteDB shared mode or off IIS means redesigning this channel.
- Peer terminals are updated over HTTP: `NotifyEntryOrExitEvent` PUTs `http://<ip>/PutTicket` to every address in `TerminalsToUpdate`.
- Exit payment chain: plate/phone/`CheckCCTransient` -> `ChargePlateResponse` -> stored as an unprocessed `PaymentAttempt` -> `checkUnprocessedPaymentsThread` (only if `PaymentServiceEnabled`) -> `ProcessTerminalPayment` -> `ProcessPaymentToApi` (upsert `Payment`, open barrier, optional receipt print, post `chargePlateResult`). `FormV4ShowReceipt` polls `PaymentProcessorService.CurrentReaderStatus` for card prompts.

### Cloud client

`ServiceLayer/PangoService.cs`: one method per endpoint, each a RestSharp POST with a 30 s timeout and the `X-QR-KEY` header. Polly is referenced in three csproj files but is never used in code, so there is no retry layer. The whitelist is polled every `WhitelistInterval` ms. Alert mail goes through `MailSender` (Microsoft Graph client credentials over `HttpClient`, never throws, silently skipped when `EmailRecipients` is empty).

### UI structure

- `ScreenForm` in `App.config` selects the start form in `Program.cs` through an if/else chain (an unknown value starts and exits silently). Families `V2`, `V3` and `V4`, each with variants `_QRTicket`, `_1opt`, `_3opt`, and `V4_Ticket_PH` (plate/phone, current for Entry; Exit is currently `V4_1opt`). V2 and V3 are older generations kept alongside V4.
- There is no shared base form. Every form derives from `Form` and repeats the same clock thread, change-screen polling thread, `InvokeRequired` boilerplate and style application. Sub-forms find their controller through a static field on each main form, selected with a `screenForm` if/else chain. Adding a variant means touching every sub-form. Entry and Exit have same-named classes in the same `AA.Pango.App` namespace with 50-80% differing lines (Exit adds payment and receipt logic), so a fix usually has to be made in both, and in V3 and V4.
- Threading is raw `new Thread` + `Control.Invoke`, with `Application.Run(form)` started on background threads for server-driven screens (`blackList`, `pangoPassAccepted`, `freeFlowEntry`).
- Styling and text are runtime files: `styles.dat` (hex colors and image names, many customer themes commented out, a missing key throws `KeyNotFound` because there are no defaults), `en.dat` / `es.dat` (`<Localized name value/>`), `languagesSupported.xml`, and Entry-only `ticketEntry.dat` (printed ticket header/footer). Images are looked up by name from `Properties/Resources.resx`. `CurrentLanguage` must be `en` or `es` (the code default `Spanish` would throw).

## Configuration

Behavior is driven by `App.config` / `Web.config` appSettings. Entry and Exit have separate configs with different key sets (Exit adds all `PaymentProcessor*`, `Receipt*`, timeout and interval keys). The connection string name is `LiteDB`. `MAX_EXPIRED_DAYS` is mandatory (bare `int.Parse` at `TicketService` class init). `PangoToken`, the Graph client secret and the RestApi `encryptAppSecurityKey` are stored in plain text in the committed config files; never copy those values into other files.

Known config traps:
- `PaymentProcessor-GlobalCom-PurchaseMaxTimeout` in `App.config` is ignored; the code reads `PaymentProcessor-GlobalCom-MaxPurchaseTimeout` and falls back to 120.
- `PaymentProcessor-AMP-PurchaseMaxTimeout` and `PaymentProcessor-AMP-UseOptimizedDeviceCommands` are defined but never read.
- Several unreferenced files are on disk but not compiled: `AA.Pango.App/V3/FormV3.Designer_0.cs`, `FormV3_CheckPlate*`, `FormV3_Ticketless*`, and Exit's `V2/FormV2.Designer - Copy*.cs`. Do not edit them thinking they are live; check the csproj `Compile Include` list.
- `InputSimulator` (`WindowsInput`) is referenced by the UI library but no code uses it. `OnScreenKeyboard.cs` there is dead COM interop.

## Windows dependencies to resolve for the Linux port

- Serial port names are built as `"COM" + n` in several places (`GlobalComPaymentManager`, AMP `DeviceCommands`, `TerminalController`, `HardResetService`) and the payment request DTO carries `ComPort` as an `int`. Linux needs `/dev/tty*` names. Mono's `SerialPort` also behaves differently, and both vendor DLLs depend on `System.IO.Ports` semantics (`DataReceived`, 115200 baud).
- `Process.Start` of Windows scripts: `RestarProcess` (`TerminalController`, `GlobalComPaymentManager`), and `RebootTerminal` / `CloseApp` in `RestController`.
- Printing uses `System.Drawing.Printing.PrintDocument` (`PangoService.PrintTicketQR`, `TerminalController.PrintInvoice`, `RestController.PrintTicket`), which assumes a Windows default printer. `RoundedButton` uses GDI+ regions, and fonts are hard-coded Calibri / Microsoft Sans Serif in the Designer files.
- Only P/Invoke in the solution: `kernel32!AttachConsole` in both `Program.cs`.
- `AA.Pango.RestApi` depends on IIS, System.Web, MVC 5, Swashbuckle.Core and WebActivatorEx. It has no authentication, and Swagger is enabled. It exposes barrier open, forced reboot and hard-reset endpoints.
- Windows paths in configs: the LiteDB file, `logs\`, `C:\AccesosAutomaticos\...`, logo paths. The AMP adapter DLL writes `\AMPLog\amppinpadsemi.log` with literal backslashes, which may yield a badly named file on Linux.
- `Picob` input parsing splits on `Environment.NewLine`, so it breaks on Linux if the device sends `\r\n`. The barcode reader uses `Encoding.Default`.
- LiteDB shared mode uses an OS-level mutex whose behavior under Mono / .NET on Linux has to be verified.
- Newtonsoft.Json is 13.0.1 in most projects and 13.0.3 in AMP; ZXing.Net is 0.16.8 vs 0.16.9 across `packages.config` files (binding redirect concern).
- Tuple syntax (`var (a, b)`) needs a Roslyn-based compiler (C# 7+), not Mono's `mcs`.

## Release history

Version notes for each app live in `AA.Pango.App/readme.txt` (Entry, latest 4.9.1.1) and `AA.Pango.App.Exit/readme.txt` (Exit, latest 4.9.4.2). Note that these are inside the project folder and part of the code that goes back to the original repos.
