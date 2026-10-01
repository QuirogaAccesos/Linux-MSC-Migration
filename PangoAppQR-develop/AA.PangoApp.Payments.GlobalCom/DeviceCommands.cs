using AA.PangoApp.Payments.Interface;
using CCI.Globalcom.GlobalcomRetailProtocol;
using log4net;
using System;
using System.Configuration;
using System.Text;
using System.Threading;

namespace AA.PangoApp.Payments.GlobalCom
{

    class DeviceCommand
    {

        private ILog log = LogManager.GetLogger("GlobalCom.DeviceCommand");

        /// <summary>
        ///  Handle to Retail Protocol
        /// </summary>
        private RetailProtocol _rp;

        /// <summary>
        ///  Implementation examples of Retail Protocol
        /// </summary>
        /// <param name="rp"> handle to initialized retail protocol</param>
        public DeviceCommand(RetailProtocol rp)
        {
            _rp = rp;
        }

        /// <summary>
        ///  Function that logs messages
        /// </summary>
        /// <param name="message">Message to be logged</param>
        private void Log(string message)
        {
            Console.WriteLine(message);
            log.Debug(message);
        }


        /// <summary>
        /// 0x01 INFO ERASE
        /// This command is used to erase the information of the status message defined as “outcome”.
        /// </summary>
        /// <param name="lang">Language to be used for prompts</param>
        /// <param name="close">This byte take the value 1 to indicate to the UPT that no more request will arrive from the last card, 
        /// this means that the operations on the card are ended, so the UPT must send the closing message to the user.
        /// This byte take the value 0 in all the other cases.</param>
        /// <exception cref="Exception"></exception>
        public void EraseInfo(ELanguage lang, bool close)
        {
            log.Debug(@"-- Erase Info --");

            RpResponseBase rpErase = _rp.RP_InfoErase(close);

            // Make sure everything is good
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(lang);
            log.Debug(@"Outcome: " + rpStatus.EBlock4.ToString());

            switch (rpErase.VerifyOutcome())
            {
                case EOutcome.Busy:
                    throw new Exception("The Terminal is busy. Please try again.");
                case EOutcome.ErrorOutcome:
                    throw new Exception("Info Erase 0x01 - Error Outcome.");
                case EOutcome.OK:
                    break;
            }
        }

        /// <summary>
        /// 0x1C Get Receipt XML
        /// Command to return the XML of the receipt to be printed by the kiosk
        /// </summary>
        /// <returns>String of XML</returns>
        public string GetReceipt()
        {
            log.Debug(@"-- Get Receipt --");

            // Get the receipt information
            string xmlReciept = _rp.RP_GetReceiptXML();
            log.Debug("--Receipt--");
            log.Debug(xmlReciept);
            return xmlReciept;
        }

        /// <summary>
        /// 0x03 TEST ETH
        /// This command is used to check the ETH configuration and perform a ping to a selected ip address
        /// </summary>
        /// <param name="ipAddress">Address to ping</param>
        public void Ping(string ipAddress)
        {
            log.Debug(@"-- Ping --");
            RpResponseBase rpTest = _rp.RP_TestEth(ipAddress);
            log.Debug(@"Ping Result: " + rpTest.VerifyOutcome().ToString());
            log.Debug(Encoding.ASCII.GetString(rpTest.Info));
        }

        /// <summary>
        /// 0x06 ENABLE With NFC 
        /// This command enables the read card via contactless, chip or mag.  It will also read 
        /// the result.  After this command you can check for loyalty cards and retrieve the card
        /// information with a separate command.
        /// </summary>
        /// <param name="timeout">Time in seconds the reader is active</param> 
        /// <param name="tran"> Handle to all the transaction settings</param>
        /// <exception cref="Exception"></exception>
        public void ReadCard(int timeout, Transaction tran)
        {

            int maxTimeoutInSeconds = int.Parse(ConfigurationManager.AppSettings["PaymentProcessor-GlobalCom-MaxPurchaseTimeout"] ?? "120");

            log.Debug(@"-- Read Card --");
            // Check violation status and fail TODO
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(tran.Lang);
            if (rpStatus.EBlock2.HasFlag(EStatusB2.AntiRemovalViolation) ||
                rpStatus.EBlock2.HasFlag(EStatusB2.PairingViolation) ||
                rpStatus.EBlock2.HasFlag(EStatusB2.SensorViolation))
            {
                throw new Exception("Reader is not enabled for processing");
            }

            // Read Card
            RpResponseBase rpEnable =
                _rp.RP_ReadCardEnableWithNFC(timeout, tran.AmountInCents,
                    tran.Currency, tran.Mode, tran.ClientTransactionId);
            if (rpEnable.VerifyOutcome() != EOutcome.OK)
            {
                _rp.RP_InfoErase(false);
                throw new Exception("Unable to Read Card." + Environment.NewLine + "Outcome:" +
                                    rpEnable.VerifyOutcome());
            }

            rpStatus = _rp.RP_StatusRequest(tran.Lang);

            // Now that command was accepted wait for response.
            int waitCount = 1;
            while (rpStatus.EBlock1 != EStatusB1.CardDataAvailable)
            {

                rpStatus = _rp.RP_StatusRequest(tran.Lang);
                log.Debug(rpStatus.EBlock4.ToString());


                if (rpStatus.EBlock1 == EStatusB1.ExpiredTimeoutOfCardRead)
                {
                    throw new Exception("Timeout Reading Card");
                }

                Thread.Sleep(500);

                if (waitCount++ > (maxTimeoutInSeconds * 2)) //final timeout of X minutes in case globalcom pos is unresponsive.
                {
                    log.Warn("GlobalCom didn't report timeout after 2 minutes, operation will be aborted.");
                    throw new Exception("Operation Aborted");
                }

            }

            //log.Debug(@"-- Read Card --");

            //RpResponseBase rpEnable =
            //    _rp.RP_ReadCardEnableWithNFC(timeout, tran.AmountInCents,
            //        tran.Currency, tran.Mode, tran.ClientTransactionId);
            //if (rpEnable.VerifyOutcome() != EOutcome.OK)
            //{
            //    _rp.RP_InfoErase(false);
            //    throw new Exception("Unable to Read Card." + Environment.NewLine + "0x05 Outcome:" +
            //                        rpEnable.VerifyOutcome());
            //}

            //RpResponseStatus rpStatus = _rp.RP_StatusRequest(tran.Lang);

            //// Now that command was accepted wait for response.
            //int waitCount = 1;
            //while (!rpStatus.EBlock2.HasFlag(EStatusB2.CardPresentAtGate))
            //{
            //    try
            //    {
            //        rpStatus = _rp.RP_StatusRequest(tran.Lang);
            //        log.Debug(rpStatus.EBlock4.ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        // try again;
            //    }

            //    Thread.Sleep(200);

            //    if (waitCount++ > 450) //90 seconds (there might be a 45 second reversal)
            //        throw new Exception("Timeout waiting for Payment Outcome.");
            //}
        }

        /// <summary>
        /// Process the card that was taped or inserted for ReadCard
        ///   0x05 PAYMENT START - This command is used to start a payment operation. 
        ///   0x0D PAYMENT OUTCOME REQUEST - The PU uses this command to request at the terminal the outcome of the payment executed.
        /// </summary>
        /// <param name="tran">Handle to all the transaction settings</param>

        public (ProcessPaymentStatus, string, string) ProcessSale(Transaction tran)
        {

            // Make sure card is ready
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(tran.Lang);

            if (rpStatus.EBlock1 != EStatusB1.CardDataAvailable)
            {
                var msg = "Card data not available. Check card and try again.";
                log.Debug(msg);
                return (ProcessPaymentStatus.Error, msg, msg);
            }


            log.Debug(@"-- Process Sale --");
            RpResponseBase rpPayment =
                _rp.RP_PaymentCommand(tran.AmountInCents, tran.Currency, tran.Mode, tran.ClientTransactionId);

            if (rpPayment.VerifyOutcome() != EOutcome.OK)
            {
                _rp.RP_InfoErase(false);
                var msg = "Unable to Process Payment." + Environment.NewLine + "0x05 Outcome:" +
                                    rpPayment.VerifyOutcome();
                log.Debug(msg);

                return (ProcessPaymentStatus.Error, msg, "Unable to Process Payment.");
            }

            int waitCount = 1;

            // Now that command was accepted wait for response.
            rpStatus = _rp.RP_StatusRequest(tran.Lang);
            while (!rpStatus.EBlock0.HasFlag(EStatusB0.TransactionOutcomeAvailable))
            {
                try
                {
                    rpStatus = _rp.RP_StatusRequest(tran.Lang);
                    log.Debug("rpStatus.EBlock4: " + rpStatus.EBlock4.ToString());
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }

                Thread.Sleep(200);

                if (waitCount++ > 450)
                {
                    //90 seconds (there might be a 45 second reversal)
                    log.Error("Timeout waiting for Payment Outcome.");
                    return (ProcessPaymentStatus.Error, "Timeout waiting for Payment Outcome.", "Timeout waiting for Payment Outcome.");
                }
            }

            log.Debug(@"-- Get Outcome --");
            RpResponsePaymentOutcome rpResult = _rp.RP_PaymentOutcomeRequest();
            log.Debug(@"Outcome: " + rpResult.VerifyOutcome().ToString());

            ProcessPaymentStatus transactionPaymentStatus = ProcessPaymentStatus.Undefined;

            var errorMsg = "";

            if (rpResult.VerifyOutcome() == EOutcome.OK)
            {

                if (rpStatus.EBlock4 == EStatusB4.TransactionOutcomeGood)
                {
                    transactionPaymentStatus = ProcessPaymentStatus.Approved;
                }
                else if (rpStatus.EBlock4 == EStatusB4.TransactionOutcomeBad)
                {
                    transactionPaymentStatus = ProcessPaymentStatus.Denied;
                }
                else if (rpStatus.EBlock4 == EStatusB4.TransactionOverCtlsFloorLimit)
                {
                    transactionPaymentStatus = ProcessPaymentStatus.Error;
                    errorMsg = "Tap Floor Limit Exceeded. Please use CHIP. ";
                }
                else
                {
                    transactionPaymentStatus = ProcessPaymentStatus.Error;
                }

            }
            else
            {
                transactionPaymentStatus = ProcessPaymentStatus.Error;
            }

            tran.ResponsePairs = rpResult.Results;

            var resultMsg = "";
            if (tran.ResponsePairs.ContainsKey("Err code"))
            {
                log.Debug(@"Err Code: " + tran.ResponsePairs["Err code"]);
                errorMsg += " Error Code: " + tran.ResponsePairs["Err code"];
            }

            if (tran.ResponsePairs.ContainsKey("Result"))
            {
                log.Debug(@"Result: " + tran.ResponsePairs["Result"]);
                resultMsg = " Result: " + tran.ResponsePairs["Result"];
                errorMsg += resultMsg;

            }

            return (transactionPaymentStatus, errorMsg, resultMsg);

        }

        /// <summary>
        /// 0x17 TODO
        /// yada yada
        /// </summary>
        /// <param name="timeout">Time in seconds the reader is active</param> 
        /// <exception cref="Exception"></exception>
        public void AnalyzeCard(bool fullDetails)
        {
            log.Debug(@"-- Analyze Card --");
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(_rp.Language);
            log.Debug(@"Card Data : " + (rpStatus.EBlock1 == EStatusB1.CardDataAvailable ? "Available" : "NotAvailable"));
            log.Debug(@"Card Present At Gate : " + (rpStatus.EBlock2.HasFlag(EStatusB2.CardPresentAtGate) ? "Yes" : "No"));
            log.Debug(@"Card Present : " + (rpStatus.EBlock0.HasFlag(EStatusB0.CardPresent) ? "Yes" : "No"));

            if (rpStatus.EBlock1 != EStatusB1.CardDataAvailable)
                return;

            if (!fullDetails)
                return;

            RpResponseTrackType rpTracks = _rp.RP_ReadTracksTypeRequest(true);

            log.Debug(@"Read Track Outcome: " + rpTracks.VerifyOutcome().ToString());

            if (rpTracks.Iso1Banking)
            {
                if (rpTracks.Iso1NotBanking)
                    log.Debug(@"ISO1 Banking Track / Whitelisted.");
                else
                    log.Debug(@"ISO1 Banking Track.");
            }
            else if (rpTracks.Iso1NotBanking)
                log.Debug(@"ISO1 Non Banking Track.");

            if (rpTracks.Iso2Banking)
            {
                if (rpTracks.Iso2NotBanking)
                    log.Debug(@"ISO2 Banking Track / Whitelisted.");
                else
                    log.Debug(@"ISO2 Banking Track.");
            }
            else if (rpTracks.Iso2NotBanking)
                log.Debug(@"ISO2 Non Banking Track.");

            if (rpTracks.HostManagesTrack2)
                log.Debug(@"Host manages the BIN on ISO2.");

            if (rpTracks.Iso3Bancomat)
            {
                if (rpTracks.Iso3NotBanking)
                    log.Debug(@"ISO3 Banocomat / Whitelisted.");
                else
                    log.Debug(@"ISO3 Bancomat.");
            }
            else if (rpTracks.Iso3NotBanking)
                log.Debug(@"ISO3 Non Banking Track.");

            if (rpTracks.ContactlessCard)
                log.Debug(@"Contactless Card.");
            if (rpTracks.EmvCard)
                log.Debug(@"Chip Card.");
            if (rpTracks.NoSlotsAvailableForPreAuth)
                log.Debug(@"This Terminal has no free Pre-Authorization slots available.");

            if (rpTracks.PromptTags.Count > 0)
            {
                log.Debug(@"Required Petroleum Prompts:");
                foreach (EPetroPromptTags tag in rpTracks.PromptTags)
                {
                    Prompting newPrompt = new Prompting()
                    {
                        Tag = tag
                    };

                    log.Debug(@"-->" + newPrompt.PromptString);
                }
            }
        }

        /// <summary>
        /// Process the card that was taped or inserted for ReadCard
        ///   0x48 PREAUTHORIZATION COMMAND - This command is used to start a pre-authorization operation. 
        ///   0x49 REQUEST OF PREAUTHORIZATION OUTCOME - The PU uses this command to request at the terminal the outcome of the preauthorization executed.
        /// </summary>
        /// <param name="tran"></param>
        /// <exception cref="Exception"></exception>
        public void ProcessPreAuth(Transaction tran)
        {
            log.Debug(@"-- Process Pre Auth --");
            RpResponseBase rpPayment =
                _rp.RP_PreAuthorization(tran.AmountInCents, tran.Currency);

            if (rpPayment.VerifyOutcome() != EOutcome.OK)
            {
                _rp.RP_InfoErase(false);
                throw new Exception("Unable to Process Payment." + Environment.NewLine + "0x05 Outcome:" +
                                    rpPayment.VerifyOutcome());
            }

            int waitCount = 1;

            // Now that command was accepted wait for response.
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(tran.Lang);
            while (!rpStatus.EBlock0.HasFlag(EStatusB0.PreauthorizationOutcomeAvailable))
            {
                try
                {
                    rpStatus = _rp.RP_StatusRequest(tran.Lang);
                    log.Debug(rpStatus.EBlock4.ToString());
                }
                catch (Exception ex)
                {
                    // likely a timeout quietly;
                }

                Thread.Sleep(200);

                if (waitCount++ > 450) //90 seconds (there might be a 45 second reversal)
                    throw new Exception("Timeout waiting for Payment Outcome.");
            }

            log.Debug(@"-- Get Outcome--");
            RpResponsePreAuthOutcome rpResult = _rp.RP_PreAuthorizationOutcome();
            log.Debug(@"Outcome: " + rpResult.VerifyOutcome().ToString());

            tran.PreAuthSlot = rpResult.PreAuthorizationIndex;

            tran.ResponsePairs = rpResult.Results;
            if (tran.ResponsePairs.ContainsKey("Result"))
                log.Debug(@"Result: " + tran.ResponsePairs["Result"]);
        }

        /// <summary>
        ///  Check if there are offlines and if there are filter through and attempt an upload.
        ///    0x30 OFFLINE TRANSACTION STATUS
        ///    This command is used to get the number of offline transactions stored on the UPT.
        ///    0x31 SETTLEMENT OFFLINE TRANSACTION
        ///    This command is used to settle the first offline transaction on the UPT.
        ///    0x32 SETTLEMENT OUTCOME REQUEST
        ///    The PU uses this command to request at the terminal the outcome of the settlement executed.
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void UploadOfflines()
        {
            log.Debug(@"-- UploadRequest --");

            // How many transactions are stored
            RpResponseMsfStatus rpMsfStatus = _rp.RP_MSFStatus();
            if (rpMsfStatus.VerifyOutcome() != EOutcome.OK)
                throw new Exception(@"Failed to get number of stored transactions. " +
                                    rpMsfStatus.VerifyOutcome().ToString());

            // Make sure there are transactions to be uploaded.
            int nTranCount = rpMsfStatus.CurrentOfflineCount;
            if (nTranCount <= 0)
                throw new Exception(@"No stored transactions");

            log.Debug(@"Upload Count is " + nTranCount.ToString());

            for (int i = 0; i < nTranCount; i++)
            {
                log.Debug(@"Upload ...");

                // Send a upload request
                RpResponseBase rpMsfSettle = _rp.RP_MSFSettle();
                switch (rpMsfSettle.VerifyOutcome())
                {
                    case EOutcome.Busy:
                        throw new Exception("The Terminal is busy. Please try again.");
                    case EOutcome.ErrorOutcome:
                        throw new Exception("Error Outcome from MSF Settlement");
                    case EOutcome.OK:
                        //party on
                        break;
                }

                int waitcount = 0;

                // wait for outcome
                RpResponseStatus rpStatus = _rp.RP_StatusRequest(_rp.Language);
                while (!(rpStatus.EBlock0.HasFlag(EStatusB0.SettlementAvailable)))
                {
                    Thread.Sleep(5000);
                    rpStatus = _rp.RP_StatusRequest(_rp.Language);
                    if (waitcount++ > 12) // Minute max
                        throw new Exception("Timeout waiting for Outcome.");
                }

                // Get transaction outcome.
                RpResponseMsfOutcome rpResult = _rp.RP_MSFSettlementOutcome();
                if (rpResult.VerifyOutcome() != EOutcome.OK)
                    throw new Exception("Failed to retrieve Settlement Outcome");

                // Track the result
                string errorCode = Encoding.ASCII.GetString(rpResult.ErrorCode);
                string reference = EndianBitConverter.Big.ToInt32(rpResult.ReferenceNumber, 0).ToString();
                log.Debug("Transaction Upload Result: " + errorCode + " with reference number: " + reference);

                // Get the receipt information
                string xmlResult = GetReceipt();

                // If the Settlement Outcome was successful continue uploading.
                if (rpResult.VerifyOutcome() == EOutcome.OK)
                {
                    log.Debug(xmlResult);
                    continue;
                }

                // For this case if it failed we are going to remove it and consider it a lost.  You can
                // check the error code (negative number for example is likely a comm issue).
                RpResponseBase rpDelete = _rp.RP_MSFDelete(0x00);
                if (rpDelete.VerifyOutcome() != EOutcome.OK)
                    throw new Exception("Transaction failed and delete attempt failed.");

                log.Debug(@"-Deleted Transaction.  Reference: " + reference);
            }
        }

        /// <summary>
        ///  Remove the preauth from its slot
        ///    0x4D Notification
        ///    This command is used  to delete a pre-authorization slot. 
        /// </summary>
        /// <param name="slot"></param>
        /// <exception cref="Exception"></exception>
        public void DeletePreAuth(int slot)
        {
            log.Debug(@"--  Delete Slot --");

            RpResponseBase rpNotify = _rp.RP_Notification(slot);
            if (rpNotify.VerifyOutcome() != EOutcome.OK)
                throw new Exception(@"Error Deleting Authorization Slot " + slot.ToString() + ". ");

            log.Debug(@"Outcome: " + rpNotify.VerifyOutcome().ToString());
        }

        /// <summary>
        ///  Post the authorization for the amount specifid
        ///    0x4A NOTIFY COMMAND
        ///     The PU uses this command to request a notify of a preauthorization, necessary to the gasoline payment.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="amountInPennies"></param>
        /// <exception cref="Exception"></exception>

        public void CompletePreAuth(int slot, decimal amountInPennies)
        {
            log.Debug(@"-- Complete PreAuth --");

            RpResponseBase rpNotify = _rp.RP_NotifyCommand((byte)slot, (int)amountInPennies);
            if (rpNotify.VerifyOutcome() != EOutcome.OK)
                throw new Exception("Error sending pre-authorization notification. " + Environment.NewLine +
                                    @"Outcome:" + rpNotify.VerifyOutcome());

            int waitCount = 1;

            //wait for Notify Outcome to be available
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(_rp.Language);
            while (!(rpStatus.EBlock0.HasFlag(EStatusB0.NotifyOutcomeAvailable)))
            {
                Thread.Sleep(500); //sleep for 500ms
                rpStatus = _rp.RP_StatusRequest(_rp.Language);
                if (waitCount++ > 60) // 30 second total wait - 500ms * 60
                    throw new Exception("Timeout waiting for Notify Outcome.");
            }

            RpResponseNotifyOutcome rpOutcome = _rp.RP_NotifyOutcome();
            log.Debug(@"Outcome: " + rpOutcome.VerifyOutcome().ToString());
        }

        /// <summary>
        ///  Reverse the last transaction processed
        ///    0x0E REFUND LAST OPERATION
        ///    This command execute the refund of the last payment executed.
        ///    0x0F RESULT OF LAST REFUND OPERATION
        ///    This command return the outcome of the refund.
        /// </summary>
        /// <param name="lang"></param>
        /// <exception cref="Exception"></exception>
        public void ReverseLastTransaction(ELanguage lang)
        {
            log.Debug(@"-- Reverse Last Transaction --");
            RpResponseBase rpRefund = _rp.RP_RefundLastOperation();
            if (rpRefund.VerifyOutcome() != EOutcome.OK)
                throw new Exception(@"Transaction Failed to Reverse");

            int waitCount = 1;

            //wait for Notify Outcome to be available
            RpResponseStatus rpStatus = _rp.RP_StatusRequest(_rp.Language);
            while (!(rpStatus.EBlock1.HasFlag(EStatusB1.CancellationOutcomeAvailable)))
            {
                Thread.Sleep(500); //sleep for 500ms
                rpStatus = _rp.RP_StatusRequest(_rp.Language);
                if (waitCount++ > 60) // 30 second total wait - 500ms * 60
                    throw new Exception("Timeout waiting for Notify Outcome.");
            }

            RpResponseBase rpRefundResult = _rp.RP_ResultOfLastRefundOperation();
            if (rpRefundResult.VerifyOutcome() != EOutcome.OK)
                throw new Exception(@"Failed to Reverse Last Transaction");

            log.Debug(@"Transaction Reversed");
        }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void GetConfigurationParam() { }

        /// <summary>
        ///  Sample Coming soon
        /// AM0RM0X17LM0I10C10-- Mux with Logging
        /// AE0R57T17L17I10C10 --regular retail protocol
        /// </summary>
        public void SetConfigurationParam() { }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        /// <returns></returns>
        public string GetSupportInfo()
        {
            return "";
        }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void CommissingUnlock() { }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void CommissingLock() { }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void PairingGetCode() { }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void PairingSet() { }

        /// <summary>
        ///  Sample Coming soon
        /// </summary>
        public void SoftwareInstall() { }
    }

    internal class MuxHandles
    {
        public MuxHandles() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        static public void Log(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        static public void RpUI_MuxDebug(object sender, MuxProtocol.MuxPacketEventArgs dArgs)
        {
            string[] txt = dArgs.MuxPacket.PrettyPrint();

            StringBuilder sb = new StringBuilder();
            sb.Append(txt[1] + " ");

            switch (dArgs.MuxPacket.Address)
            {
                case EMuxAddress.AckNak:
                    return;  // Supress the logging
                case EMuxAddress.Banking:
                    sb.Append("Banking");
                    break;
                case EMuxAddress.Control:
                    sb.Append("Control");
                    break;
                case EMuxAddress.RetailProtocol:
                    sb.Append("RP");
                    break;
                default:
                    return; // supress the logging
            }

            switch (dArgs.MuxPacket.Cmd)
            {
                case ECmd.Ack:
                    sb.Append("-Ack");
                    break;
                case ECmd.AckAndApplication:
                    sb.Append("-AckAndApplication");
                    break;
                case ECmd.Application:
                    sb.Append("-Application");
                    break;
                case ECmd.Nak:
                    sb.Append("-NAK");
                    break;
            }

            sb.Append(" " + txt[7]);
            //log.Debug(sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        static public void RpUI_SysLog(object sender, MuxProtocol.SyslogEventArgs dArgs)
        {
            //log.Debug(dArgs.SyslogMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        static public void RpUI_MuxLogDebug(object sender, MuxProtocol.MuxLogMessageEventArgs dArgs)
        {
            string slog = DateTime.Now.ToString("hh:mm:ss.fff") + " :" + dArgs.LogMessage;
            // log.Debug(slog);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        static public void RpUI_RefreshUI(object sender) //, RetailProtocol.DisplayArgs dArgs)
        {
            // Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        static public void RpUI_MuxBankingDataLog(object sender, MuxProtocol.MuxBankingDataLogMessageEventArgs dArgs)
        {
            string sLog = DateTime.Now.ToString("hh:mm:ss.fff") + " :" + dArgs.LogMessage;
            // log.Debug(sLog);
            string soutputflag = "OutputFlag" + ((dArgs.Outputflag) ? "Yes" : "No");
            // log.Debug(soutputflag);
        }
    }
}
