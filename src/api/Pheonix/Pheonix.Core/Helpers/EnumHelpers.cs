using System;
using System.ComponentModel;
using System.Reflection;

namespace Pheonix.Core.Helpers
{
    public class EnumHelpers
    {
        public enum Location
        {
            [Description("Mumbai, India")]
            Mumbai = 0,

            [Description("Bangalore, India")]
            Bangalore = 1,

            [Description("Santa Clara, USA")]
            SantaClara = 2
        }

        public enum WorkLocation
        {
            [Description("U.S.A - Santa Clara")]
            SantaClara = 1,

            [Description("India - Mumbai")]
            Mumbai = 2,

            [Description("India - Bengaluru")]
            Bengaluru = 3,

            [Description("Client Location")]
            Client = 4,

            [Description("U.S. – Bay Area")]
            BayArea = 5,

            [Description("U.S. – Los Angeles")]
            LosAngeles = 6,

            [Description("U.S. – Seattle")]
            Seattle = 7,

            [Description("U.S. – TN")]
            TN = 8,

            [Description("ALL")]
            ALL = 9,

            [Description("US")]
            US = 10,

            [Description("Mumbai/Bengaluru")]
            MumbaiBengaluru = 11,

            [Description("Charlotte-North Carolina")]
            Carolina = 12

        }

        public enum EmailTemplateType
        {
            [Description("ExpenseApproval")]
            ExpenseApproval = 1,

            [Description("UserProfile")]
            UserProfile = 2,

            [Description("UserProfileStatus")]
            UserProfileStatus = 3,

            [Description("Attendance")]
            Attendance = 4,

            [Description("Leave")]
            Leave = 5,

            [Description("LeaveStatus")]
            LeaveStatus = 6,

            [Description("Helpdesk")]
            Helpdesk = 7,

            [Description("ApprasalInitiation")]
            ApprasalInitiation = 8,

            [Description("ApprovalSummary")]
            ApprovalSummary = 9,

            [Description("TravelApproval")]
            TravelApproval = 10,

            [Description("Separation")]
            Separation = 12,

            [Description("Confirmation")]
            Confirmation = 11,

            [Description("ConfrimationReminder")]
            ConfrimationReminder = 13,

            [Description("ApprovalSummaryContent")]
            ApprovalSummaryContent = 14,

            [Description("SeparationApplication")]
            SeparationApplication = 15,

            [Description("HRResignationApproval")]
            HRResignationApproval = 16,

            [Description("EmployeeWithdrawResignation")]
            EmployeeWithdrawResignation = 17,

            [Description("EPMApproveWithdrawRequest")]
            EPMApproveWithdrawRequest = 18,

            [Description("EPMRejectWithdrawRequest")]
            EPMRejectWithdrawRequest = 19,

            [Description("SeparationProcessInitiated")]
            SeparationProcessInitiated = 20,

            [Description("DepartmentClearance")]
            DepartmentClearance = 21,

            [Description("SeparationProcessClosed")]
            SeparationProcessClosed = 22,

            [Description("EPMResignationApproval")]
            EPMResignationApproval = 23,

            [Description("ToDoList")]
            ToDoList = 24,

            [Description("NPExtension")]
            NPExtension = 25,

            [Description("NoExtension")]
            NoExtension = 26,

            [Description("ReleaseDateChange")]
            ReleaseDateChange = 27,

            [Description("TempAccessBlockForHRSeparation")]
            TempAccessBlockForHRSeparation = 28,

            [Description("AbscSCN1")]
            AbscSCN1 = 29,

            [Description("HRSeparationCompleted")]
            HRSeparationCompleted = 30,

            [Description("LWPNotification")]
            LWPNotification = 31,

            [Description("WithoutSettlementSCN1")]
            WithoutSettlementSCN1 = 32,

            [Description("HRSeparationInitiation")]
            HRSeparationInitiation = 33,

            [Description("TempAccessBlockForEmpResignation")]
            TempAccessBlockForEmpResignation = 34,

            [Description("SCN2Type1")]
            SCN2Type1 = 35,

            [Description("SCN2Type2")]
            SCN2Type2 = 36,

            [Description("SCN2Type3")]
            SCN2Type3 = 37,

            [Description("SCN2Type4")]
            SCN2Type4 = 38,

            [Description("SCN2Type5")]
            SCN2Type5 = 39,

            [Description("SeparationReminderContent")]
            SeparationReminderContent = 40,

            [Description("SeparationReminderMail")]
            SeparationReminderMail = 41,

            [Description("FillExitForm")]
            FillExitForm = 42,

            [Description("ExitFormSubmitted")]
            ExitFormSubmitted = 43,

            [Description("IntimationToEPM")]
            IntimationToEPM = 44,

            [Description("DeptClearanceByHR")]
            DeptClearanceByHR = 45,

            [Description("OnPIP")]
            OnPIP = 46,

            [Description("ApprovalSummaryMail")]
            ApprovalSummaryMail = 47,

            [Description("EmployeeNotReporting")]
            EmployeeNotReporting = 48,

            [Description("AbscSCN2Type1")]
            AbscSCN2Type1 = 49,

            [Description("SeparationApprovalContent")]
            SeparationApprovalContent = 50,

            [Description("ProjectCreation")]
            ProjectCreation = 51,

            [Description("ProjectUpdate")]
            ProjectUpdate = 52,

            [Description("CustomerCreation")]
            CustomerCreation = 53,

            [Description("MangersubmittedIR")]
            MangersubmittedIR = 54,

            [Description("IRApproves")]
            IRApproves = 55,

            [Description("IRRejects")]
            IRRejects = 56,

            [Description("IROnHold")]
            IROnHold = 57,

            [Description("RARequestRaised")]
            RARequestRaised = 58,

            [Description("RARequestUpdated")]
            RARequestUpdated = 59,

            [Description("RAAlloocationUpdate")]
            RAAlloocationUpdate = 60,

            [Description("RAAction")]
            RAAction = 61,

            [Description("RARelease")]
            RARelease = 62,

            [Description("UpdateCustomer")]
            UpdateCustomer = 63,

            [Description("CustomerContractEndDatereminder")]
            CustomerContractEndDatereminder = 64,

            [Description("ProjectEndDatereminder")]
            ProjectEndDatereminder = 65,

            [Description("RAEndDateReminder")]
            RAEndDateReminder = 66,

            [Description("NewEmployee")]
            NewEmployee = 67,

            [Description("JoiningUpdate")]
            JoiningUpdate = 68,

            [Description("SendForApproval")]
            SendForApproval = 69,

            [Description("ApproveRRF")]
            ApproveRRF = 70,

            [Description("RejectRRF")]
            RejectRRF = 71,

            [Description("ApprovedRRFHR")]
            ApprovedRRFHR = 72,

            [Description("CancelRRF")]
            CancelRRF = 73,

            [Description("CloseRRF")]
            CloseRRF = 74,

            [Description("RRFSwap")]
            RRFSwap = 75,

            [Description("ContractEndInitiate")]
            ContractEndInitiate = 90,

            [Description("ContractComplete")]
            ContractComplete = 91,

            [Description("BGUPdatetoHR")]
            BGUPdatetoHR = 94,

            [Description("BGUPdatetoRMG")]
            BGUPdatetoRMG = 95,

            [Description("ValuePortal")]
            ValuePortal = 96,

            [Description("ValuePortalIdeaSubmission")]
            ValuePortalIdeaSubmission = 97,

            [Description("ValuePortalIdeaStatus")]
            ValuePortalIdeaStatus = 98,

            [Description("VPSubmitPhaseOne")]
            VPSubmitPhaseOne = 99,

            [Description("VCFUpdate")]
            VCFUpdate = 100,

            [Description("VCFNewCommentAdded")]
            VCFNewCommentAdded = 101,

            [Description("VCFSubmitPhaseTwo")]
            VCFSubmitPhaseTwo = 102,

            [Description("CelebrationList")]
            CelebrationList = 103,

            [Description("RMandEMUpdate")]
            RMandEMUpdate = 104,

            [Description("RAPercentageDetails")]
            RAPercentageDetails = 105,

            [Description("LeaveCancellationUpdate")]
            LeaveCancellationUpdate = 106,
        }

        public enum ApprovalType
        {
            Leave,
            UserProfile,
            Expense,
            Attendance
        }

        public enum HelpdeskSeverity
        {
            [Description("High")]
            High = 0,

            [Description("Medium")]
            Medium = 1,

            [Description("Low")]
            Low = 2
        }

        public enum HelpdeskStatus
        {
            [Description("Pending For Approval")]
            PendingForApproval = 1,

            [Description("Open")]
            Open = 2,

            [Description("Rejected")]
            Rejected = 3,

            [Description("In Progress")]
            InProgress = 4,

            [Description("On Hold")]
            OnHold = 5,

            [Description("Resolved")]
            Resolved = 6,

            [Description("Cancelled")]
            Cancelled = 7,
        }

        public enum TripType
        {
            [Description("One Way Trip")]
            OneWayTrip = 1,

            [Description("Round Trip")]
            RoundTrip = 2
        }

        public enum TravelType
        {
            [Description("Domestic")]
            Domestic = 1,

            [Description("International")]
            International = 2
        }

        public enum RequestType
        {
            [Description("Travel")]
            Travel = 1,

            [Description("Accommodation only")]
            Accommodation = 2,

            [Description("Travel with Accommodation")]
            TravelWithAccommodation = 3
        }
    }

    public static class EnumExtensions
    {
        public static string GetEnumDescription(Enum enumValue)
        {
            string enumValueAsString = enumValue.ToString();

            var type = enumValue.GetType();
            FieldInfo fieldInfo = type.GetField(enumValueAsString);
            if (fieldInfo != null)
            {
                object[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    var attribute = (DescriptionAttribute)attributes[0];
                    return attribute.Description;
                }
            }

            return enumValueAsString;
        }
    }
}