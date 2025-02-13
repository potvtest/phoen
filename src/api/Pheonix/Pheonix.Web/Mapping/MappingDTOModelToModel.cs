using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.Report;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.Models.AdminConfig;
using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes;
using Pheonix.Models.VM.Classes.Appraisal;
using Pheonix.Models.VM.Classes.Employee;
using Pheonix.Models.VM.Classes.HelpDesk;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using Pheonix.Models.VM.Classes.Travel;
using System;
using System.Linq;
using vm = Pheonix.Models.ViewModels;

namespace Pheonix.Web.Mapping
{
    public static class MappingDTOModelToModel
    {
        public static void Configure()
        { //ConfigureUsingMapping();
            ConfigureEmployee();
        }

        private static void ConfigureUsingMapping()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Person, PersonViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.FirstName,
                               m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.LastName,
                               m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.MiddleName,
                               m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Gender,
                               m => m.MapFrom(a => a.Gender))
                    .ForMember(x => x.Salutation,
                               m => m.MapFrom(a => a.Salutation))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.PersonEmployment,
                               m => m.Ignore())
                    .ForMember(x => x.EmploymentStatus,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().EmploymentStatus : 0));

                cfg.CreateMap<PersonViewModel, Person>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.FirstName,
                               m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.LastName,
                               m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.MiddleName,
                               m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Gender,
                               m => m.MapFrom(a => a.Gender))
                    .ForMember(x => x.Salutation,
                               m => m.MapFrom(a => a.Salutation))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.PersonEmployment,
                               m => m.Ignore());

                cfg.CreateMap<PersonEmployment, PersonEmploymentViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.JoiningDate,
                               m => m.MapFrom(a => a.JoiningDate))
                    .ForMember(x => x.DesignationID,
                               m => m.MapFrom(a => a.DesignationID))
                    .ForMember(x => x.ProbationMonths,
                                m => m.MapFrom(a => a.ProbationMonths))
                    .ForMember(x => x.ProbationReviewDate,
                               m => m.MapFrom(a => a.ProbationReviewDate))
                    .ForMember(x => x.ConfirmationDate,
                                m => m.MapFrom(a => a.ConfirmationDate))
                    .ForMember(x => x.SeparationRequestDate,
                                m => m.MapFrom(a => a.SeparationRequestDate))
                    .ForMember(x => x.ExitDate,
                               m => m.MapFrom(a => a.ExitDate))
                    .ForMember(x => x.RejoinedWithinYear,
                                m => m.MapFrom(a => a.RejoinedWithinYear))
                    .ForMember(x => x.EmployeeType,
                                m => m.MapFrom(a => a.EmployeeType))
                    .ForMember(x => x.EmploymentStatus,
                               m => m.MapFrom(a => a.EmploymentStatus))
                    .ForMember(x => x.UserName,
                                m => m.MapFrom(a => a.UserName))
                    .ForMember(x => x.BusinessGroup,
                                m => m.MapFrom(a => a.BusinessGroup))
                    .ForMember(x => x.OrgUnit,
                               m => m.MapFrom(a => a.OrgUnit))
                    .ForMember(x => x.CurrentDU,
                                m => m.MapFrom(a => a.CurrentDU))
                    .ForMember(x => x.DeliveryTeam,
                                m => m.MapFrom(a => a.DeliveryTeam))
                    .ForMember(x => x.ResourcePool,
                               m => m.MapFrom(a => a.ResourcePool))
                    .ForMember(x => x.Commitment,
                                m => m.MapFrom(a => a.Commitment))
                    .ForMember(x => x.OfficeExtension,
                                m => m.MapFrom(a => a.OfficeExtension))
                    .ForMember(x => x.SeatingLocation,
                               m => m.MapFrom(a => a.SeatingLocation))
                    .ForMember(x => x.OrganizationEmail,
                               m => m.MapFrom(a => a.OrganizationEmail))
                    .ForMember(x => x.CompetencyID,
                               m => m.MapFrom(a => a.CompetencyID))
                    .ForMember(x => x.SkillDescription,
                               m => m.MapFrom(a => a.SkillDescription));

                cfg.CreateMap<PersonEmploymentViewModel, PersonEmployment>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.JoiningDate,
                               m => m.MapFrom(a => a.JoiningDate))
                    .ForMember(x => x.DesignationID,
                               m => m.MapFrom(a => a.DesignationID))
                    .ForMember(x => x.ProbationMonths,
                                m => m.MapFrom(a => a.ProbationMonths))
                    .ForMember(x => x.ProbationReviewDate,
                               m => m.MapFrom(a => a.ProbationReviewDate))
                    .ForMember(x => x.ConfirmationDate,
                                m => m.MapFrom(a => a.ConfirmationDate))
                    .ForMember(x => x.SeparationRequestDate,
                                m => m.MapFrom(a => a.SeparationRequestDate))
                    .ForMember(x => x.ExitDate,
                               m => m.MapFrom(a => a.ExitDate))
                    .ForMember(x => x.RejoinedWithinYear,
                                m => m.MapFrom(a => a.RejoinedWithinYear))
                    .ForMember(x => x.EmployeeType,
                                m => m.MapFrom(a => a.EmployeeType))
                    .ForMember(x => x.EmploymentStatus,
                               m => m.MapFrom(a => a.EmploymentStatus))
                    .ForMember(x => x.UserName,
                                m => m.MapFrom(a => a.UserName))
                    .ForMember(x => x.BusinessGroup,
                                m => m.MapFrom(a => a.BusinessGroup))
                    .ForMember(x => x.OrgUnit,
                               m => m.MapFrom(a => a.OrgUnit))
                    .ForMember(x => x.CurrentDU,
                                m => m.MapFrom(a => a.CurrentDU))
                    .ForMember(x => x.DeliveryTeam,
                                m => m.MapFrom(a => a.DeliveryTeam))
                    .ForMember(x => x.ResourcePool,
                               m => m.MapFrom(a => a.ResourcePool))
                    .ForMember(x => x.Commitment,
                                m => m.MapFrom(a => a.Commitment))
                    .ForMember(x => x.OfficeExtension,
                                m => m.MapFrom(a => a.OfficeExtension))
                    .ForMember(x => x.SeatingLocation,
                               m => m.MapFrom(a => a.SeatingLocation))
                    .ForMember(x => x.OrganizationEmail,
                               m => m.MapFrom(a => a.OrganizationEmail))
                    .ForMember(x => x.CompetencyID,
                               m => m.MapFrom(a => a.CompetencyID))
                    .ForMember(x => x.SkillDescription,
                               m => m.MapFrom(a => a.SkillDescription));

                cfg.CreateMap<PersonSkillMapping, PersonSkillMappingViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.SkillID,
                               m => m.MapFrom(a => a.SkillID))
                    .ForMember(x => x.ExperienceYears,
                               m => m.MapFrom(a => a.ExperienceYears))
                    .ForMember(x => x.ExperienceMonths,
                               m => m.MapFrom(a => a.ExperienceMonths))
                    .ForMember(x => x.HasCoreCompetency,
                               m => m.MapFrom(a => a.HasCoreCompetency))
                    .ForMember(x => x.SkillRating,
                               m => m.MapFrom(a => a.SkillRating))
                    .ForMember(x => x.IsPrimary,
                               m => m.MapFrom(a => a.IsPrimary));

                cfg.CreateMap<PersonSkillMappingViewModel, PersonSkillMapping>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.SkillID,
                               m => m.MapFrom(a => a.SkillID))
                    .ForMember(x => x.ExperienceYears,
                               m => m.MapFrom(a => a.ExperienceYears))
                    .ForMember(x => x.ExperienceMonths,
                               m => m.MapFrom(a => a.ExperienceMonths))
                    .ForMember(x => x.HasCoreCompetency,
                               m => m.MapFrom(a => a.HasCoreCompetency))
                    .ForMember(x => x.SkillRating,
                               m => m.MapFrom(a => a.SkillRating))
                    .ForMember(x => x.IsPrimary,
                               m => m.MapFrom(a => a.IsPrimary));

                cfg.CreateMap<SkillMatrix, SkillMatrixViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.SkillCategory,
                               m => m.MapFrom(a => a.SkillCategory));

                cfg.CreateMap<SkillMatrixViewModel, SkillMatrix>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.SkillCategory,
                               m => m.MapFrom(a => a.SkillCategory));

                cfg.CreateMap<PersonAddress, PersonAddressViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.City,
                               m => m.MapFrom(a => a.City))
                    .ForMember(x => x.State,
                               m => m.MapFrom(a => a.State))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.Pin,
                               m => m.MapFrom(a => a.Pin))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.IsCurrent))
                    .ForMember(x => x.HouseOnRent,
                               m => m.MapFrom(a => a.HouseOnRent))
                    .ForMember(x => x.ProofSubmitted,
                               m => m.MapFrom(a => a.ProofSubmitted))
                    .ForMember(x => x.ProofLocation,
                               m => m.MapFrom(a => a.ProofLocation))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<PersonAddressViewModel, PersonAddress>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.City,
                               m => m.MapFrom(a => a.City))
                    .ForMember(x => x.State,
                               m => m.MapFrom(a => a.State))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.Pin,
                               m => m.MapFrom(a => a.Pin))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.IsCurrent))
                    .ForMember(x => x.HouseOnRent,
                               m => m.MapFrom(a => a.HouseOnRent))
                    .ForMember(x => x.ProofSubmitted,
                               m => m.MapFrom(a => a.ProofSubmitted))
                    .ForMember(x => x.ProofLocation,
                               m => m.MapFrom(a => a.ProofLocation))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<PersonContact, PersonContactViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.Relation,
                               m => m.MapFrom(a => a.Relation))
                    .ForMember(x => x.Email,
                               m => m.MapFrom(a => a.Email))
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.City,
                               m => m.MapFrom(a => a.City))
                    .ForMember(x => x.State,
                               m => m.MapFrom(a => a.State))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.Zip,
                               m => m.MapFrom(a => a.Zip))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<PersonContactViewModel, PersonContact>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.Relation,
                               m => m.MapFrom(a => a.Relation))
                    .ForMember(x => x.Email,
                               m => m.MapFrom(a => a.Email))
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.City,
                               m => m.MapFrom(a => a.City))
                    .ForMember(x => x.State,
                               m => m.MapFrom(a => a.State))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.Zip,
                               m => m.MapFrom(a => a.Zip))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<PersonPersonal, PersonPersonalViewModel>()
                    .ForMember(x => x.BloodGroup,
                               m => m.MapFrom(a => a.BloodGroup))
                    .ForMember(x => x.MaritalStatus,
                               m => m.MapFrom(a => a.MaritalStatus))
                    .ForMember(x => x.SpouseName,
                               m => m.MapFrom(a => a.SpouseName))
                    .ForMember(x => x.SpouseBirthDate,
                               m => m.MapFrom(a => a.SpouseBirthDate))
                    .ForMember(x => x.DateOfMarriage,
                               m => m.MapFrom(a => a.DateOfMarriage))
                    .ForMember(x => x.FatherName,
                               m => m.MapFrom(a => a.FatherName))
                    .ForMember(x => x.MotherName,
                               m => m.MapFrom(a => a.MotherName))
                    .ForMember(x => x.PANNo,
                               m => m.MapFrom(a => a.PANNo))
                    .ForMember(x => x.PFNo,
                               m => m.MapFrom(a => a.PFNo))
                    .ForMember(x => x.ESICNo,
                               m => m.MapFrom(a => a.ESICNo))
                    .ForMember(x => x.PersonalEmail,
                               m => m.MapFrom(a => a.PersonalEmail))
                    .ForMember(x => x.Hobbies,
                               m => m.MapFrom(a => a.Hobbies))
                    .ForMember(x => x.NoofChildren,
                               m => m.MapFrom(a => a.NoofChildren))
                    .ForMember(x => x.Achievement,
                               m => m.MapFrom(a => a.Achievement))
                    .ForMember(x => x.PhotoFileName,
                               m => m.MapFrom(a => a.PhotoFileName))
                    .ForMember(x => x.PhotoFilePath,
                               m => m.MapFrom(a => a.PhotoFilePath))
                    .ForMember(x => x.Phone,
                               m => m.MapFrom(a => a.Phone))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.AlternateEmail,
                               m => m.MapFrom(a => a.AlternateEmail))
                    .ForMember(x => x.AlternateContactNo,
                               m => m.MapFrom(a => a.AlternateContactNo))
                    .ForMember(x => x.VOIPNo,
                               m => m.MapFrom(a => a.VOIPNo))
                    .ForMember(x => x.GtalkID,
                               m => m.MapFrom(a => a.GtalkID))
                    .ForMember(x => x.SkypeID,
                               m => m.MapFrom(a => a.SkypeID))
                    .ForMember(x => x.YIMID,
                               m => m.MapFrom(a => a.YIMID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<PersonPersonalViewModel, PersonPersonal>()
                    .ForMember(x => x.BloodGroup,
                               m => m.MapFrom(a => a.BloodGroup))
                    .ForMember(x => x.MaritalStatus,
                               m => m.MapFrom(a => a.MaritalStatus))
                    .ForMember(x => x.SpouseName,
                               m => m.MapFrom(a => a.SpouseName))
                    .ForMember(x => x.SpouseBirthDate,
                               m => m.MapFrom(a => a.SpouseBirthDate))
                    .ForMember(x => x.DateOfMarriage,
                               m => m.MapFrom(a => a.DateOfMarriage))
                    .ForMember(x => x.FatherName,
                               m => m.MapFrom(a => a.FatherName))
                    .ForMember(x => x.MotherName,
                               m => m.MapFrom(a => a.MotherName))
                    .ForMember(x => x.PANNo,
                               m => m.MapFrom(a => a.PANNo))
                    .ForMember(x => x.PFNo,
                               m => m.MapFrom(a => a.PFNo))
                    .ForMember(x => x.ESICNo,
                               m => m.MapFrom(a => a.ESICNo))
                    .ForMember(x => x.PersonalEmail,
                               m => m.MapFrom(a => a.PersonalEmail))
                    .ForMember(x => x.Hobbies,
                               m => m.MapFrom(a => a.Hobbies))
                    .ForMember(x => x.NoofChildren,
                               m => m.MapFrom(a => a.NoofChildren))
                    .ForMember(x => x.Achievement,
                               m => m.MapFrom(a => a.Achievement))
                    .ForMember(x => x.PhotoFileName,
                               m => m.MapFrom(a => a.PhotoFileName))
                    .ForMember(x => x.PhotoFilePath,
                               m => m.MapFrom(a => a.PhotoFilePath))
                    .ForMember(x => x.Phone,
                               m => m.MapFrom(a => a.Phone))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.AlternateEmail,
                               m => m.MapFrom(a => a.AlternateEmail))
                    .ForMember(x => x.AlternateContactNo,
                               m => m.MapFrom(a => a.AlternateContactNo))
                    .ForMember(x => x.VOIPNo,
                               m => m.MapFrom(a => a.VOIPNo))
                    .ForMember(x => x.GtalkID,
                               m => m.MapFrom(a => a.GtalkID))
                    .ForMember(x => x.SkypeID,
                               m => m.MapFrom(a => a.SkypeID))
                    .ForMember(x => x.YIMID,
                               m => m.MapFrom(a => a.YIMID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID));
            });
        }

        private static void ConfigureEmployee()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ReportConfirmationDataModel, Confirmations>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.JoiningDate,
                              m => m.MapFrom(a => a.JoiningDate))
                   .ForMember(x => x.ReviewDate,
                              m => m.MapFrom(a => a.ReviewDate))
                   .ForMember(x => x.InitiatedDate,
                              m => m.MapFrom(a => a.InitiatedDate))
                   .ForMember(x => x.PersonId,
                              m => m.MapFrom(a => a.PersonId))
                   .ReverseMap();

                cfg.CreateMap<ReportConfirmationDataModel, ConfirmationFeedback>()
                   .ForMember(x => x.ConfirmationState,
                              m => m.MapFrom(a => a.ConfirmationStatus))
                   .ForMember(x => x.TrainingFeedback,
                              m => m.MapFrom(a => a.TrainingFeedback))
                   .ForMember(x => x.BehaviourFeedback,
                              m => m.MapFrom(a => a.BehaviourFeedback))
                   .ForMember(x => x.OverallFeedback,
                              m => m.MapFrom(a => a.OverallFeedback))
                   .ForMember(x => x.ExtendedTill,
                              m => m.MapFrom(a => a.ExtendedTill))
                   .ForMember(x => x.PIPTill,
                              m => m.MapFrom(a => a.PIPTill))
                   .ReverseMap();

                cfg.CreateMap<ReportConfirmationDataModel, EmployeeBasicProfile>()
                   .ForMember(x => x.FirstName,
                              m => m.MapFrom(a => a.FirstName))
                   .ForMember(x => x.LastName,
                              m => m.MapFrom(a => a.LastName))
                   .ReverseMap();

                cfg.CreateMap<PersonConfirmation, Confirmations>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.ReviewDate,
                              m => m.MapFrom(a => a.ReviewDate))
                   .ForMember(x => x.InitiatedDate,
                              m => m.MapFrom(a => a.InitiatedDate))
                   .ForMember(x => x.PersonId,
                              m => m.MapFrom(a => a.PersonId))
                   .ReverseMap();

                cfg.CreateMap<PersonConfirmation, ConfirmationFeedback>()
                   .ForMember(x => x.TrainingFeedback,
                              m => m.MapFrom(a => a.TrainingFeedback))
                   .ForMember(x => x.BehaviourFeedback,
                              m => m.MapFrom(a => a.BehaviourFeedback))
                   .ForMember(x => x.OverallFeedback,
                              m => m.MapFrom(a => a.OverallFeedback))
                   .ForMember(x => x.ConfirmationState,
                              m => m.MapFrom(a => a.ConfirmationState))
                   .ForMember(x => x.ExtendedTill,
                              m => m.MapFrom(a => a.ExtendedTill))
                   .ForMember(x => x.PIPTill,
                              m => m.MapFrom(a => a.PIPTill))
                   .ForMember(x => x.IsHRReviewDone,
                              m => m.MapFrom(a => a.IsHRReviewDone))
                   //.ForMember(x => x.PersonId,
                   //           m => m.MapFrom(a => a.PersonId))
                   .ReverseMap();

                cfg.CreateMap<Person, PersonViewModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.FirstName,
                              m => m.MapFrom(a => a.FirstName))
                   .ForMember(x => x.LastName,
                              m => m.MapFrom(a => a.LastName))
                   .ForMember(x => x.MiddleName,
                              m => m.MapFrom(a => a.MiddleName))
                   .ForMember(x => x.DateOfBirth,
                              m => m.MapFrom(a => a.DateOfBirth))
                   .ForMember(x => x.Gender,
                              m => m.MapFrom(a => a.Gender))
                   .ForMember(x => x.Salutation,
                              m => m.MapFrom(a => a.Salutation))
                   .ForMember(x => x.Active,
                              m => m.MapFrom(a => a.Active))
                   .ForMember(x => x.PersonEmployment,
                              m => m.Ignore())
                   .ForMember(x => x.EmploymentStatus,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().EmploymentStatus : 0));

                cfg.CreateMap<PersonViewModel, Person>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.FirstName,
                               m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.LastName,
                               m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.MiddleName,
                               m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Gender,
                               m => m.MapFrom(a => a.Gender))
                    .ForMember(x => x.Salutation,
                               m => m.MapFrom(a => a.Salutation))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.PersonEmployment,
                               m => m.Ignore());

                cfg.CreateMap<Person, EmployeeProfileViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.FirstName,
                               m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.MiddleName,
                               m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.LastName,
                               m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.CurrentDesignationID,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().DesignationID : 0))
                    .ForMember(x => x.CurrentDesignation,
                    m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   ((a.PersonEmployment.First().Designation.Name.Contains("Consultant -")) ? a.PersonEmployment.First().Designation.Name.Trim().Split('-')[0] : a.PersonEmployment.First().Designation.Name)
                                   : string.Empty))
                    .ForMember(x => x.Commitment,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().Commitment : string.Empty))
                    .ForMember(x => x.PFNo,
                               m => m.MapFrom(a => a.PersonPersonal.PFNo))
                    .ForMember(x => x.PANNo,
                               m => m.MapFrom(a => a.PersonPersonal.PANNo))
                    .ForMember(x => x.Passport,
                               m => m.MapFrom(a =>
                                   a.PersonPassport.Any() ?
                                       a.PersonPassport.Where(t => t.RelationWithPPHolder == 1)
                                           .First().PassportNumber : null))
                    .ForMember(x => x.UserName,
                               m => m.MapFrom(a => a.PersonEmployment.First().UserName))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Gender,
                               m => m.MapFrom(a => a.Gender))
                    .ForMember(x => x.MaritalStatus,
                               m => m.MapFrom(a => a.PersonPersonal.MaritalStatus))
                    .ForMember(x => x.WeddingDate,
                               m => m.MapFrom(a => a.PersonPersonal.DateOfMarriage))
                    .ForMember(x => x.SpouseName,
                               m => m.MapFrom(a => a.PersonPersonal.SpouseName))
                    .ForMember(x => x.SpouseBirthDate,
                               m => m.MapFrom(a => a.PersonPersonal.SpouseBirthDate))
                    .ForMember(x => x.NoofChildren,
                               m => m.MapFrom(a => a.PersonPersonal.NoofChildren))
                    .ForMember(x => x.Hobbies,
                               m => m.MapFrom(a => a.PersonPersonal.Hobbies))
                    .ForMember(x => x.FirstChildName,
                               m => m.MapFrom(a =>
                                   a.PersonDependents.Where(p => p.Relation == 6 || p.Relation == 7).Any() ?
                                        a.PersonDependents.Where(p => p.Relation == 6 || p.Relation == 7)
                                            .OrderBy(p => p.DateOfBirth).First().Name : null))
                    .ForMember(x => x.FirstChildDateOfBirth,
                               m => m.MapFrom(a =>
                                   a.PersonDependents.Where(p => p.Relation == 6 || p.Relation == 7).Any() ?
                                        a.PersonDependents.Where(p => p.Relation == 6 || p.Relation == 7)
                                            .OrderBy(p => p.DateOfBirth).First().DateOfBirth : null))
                    .ForMember(x => x.ResidenceNumber,
                               m => m.MapFrom(a => a.PersonPersonal.Phone))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.PersonPersonal.Mobile))
                    .ForMember(x => x.PersonalEmail,
                               m => m.MapFrom(a => a.PersonPersonal.PersonalEmail))
                    .ForMember(x => x.JoiningDate,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().JoiningDate : null))
                    .ForMember(x => x.ConfirmationDate,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().ConfirmationDate : null))
                    .ForMember(x => x.ProbationReviewDate,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().ProbationReviewDate : null))
                    .ForMember(x => x.ExitDate,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().ExitDate : null))
                    .ForMember(x => x.RejoinedWithinYear,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().RejoinedWithinYear : false))
                    .ForMember(x => x.ReportingTo,
                               m => m.Ignore())
                    .ForMember(x => x.CompetencyManager,
                               m => m.Ignore())
                    .ForMember(x => x.ExitProcessManager,
                               m => m.Ignore())
                    .ForMember(x => x.OrganizationEmail,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().OrganizationEmail : string.Empty))
                    .ForMember(x => x.OL,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().OfficeLocation : 0))
                    .ForMember(x => x.OLText,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ? EnumExtensions.GetEnumDescription((EnumHelpers.Location)a.PersonEmployment.First().OfficeLocation) : string.Empty))
                    .ForMember(x => x.ImagePath,
                               m => m.MapFrom(a => a.Image))
                    .ForMember(x => x.SeatingLocation,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   (a.PersonEmployment.First().SeatingLocation != null ? a.PersonEmployment.First().SeatingLocation : string.Empty) : string.Empty))
                    .ForMember(x => x.bloodGroup,
                               m => m.MapFrom(a => a.PersonPersonal.BloodGroup))
                     .ReverseMap();

                cfg.CreateMap<PersonEmployment, EmployeeOrganizaionDetails>()
                    .ForMember(x => x.OrgUnit,
                               m => m.MapFrom(a => a.OrgUnit))
                    .ForMember(x => x.DeliveryUnit,
                               m => m.MapFrom(a => a.DeliveryUnit))
                    .ForMember(x => x.CurrentDU,
                               m => m.MapFrom(a => a.CurrentDU))
                    .ForMember(x => x.DeliveryTeam,
                               m => m.MapFrom(a => a.DeliveryTeam))
                    .ForMember(x => x.ResourcePool,
                               m => m.MapFrom(a => a.ResourcePool))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.Person.ID))
                    .ForMember(x => x.ExitProcessManager,
                               m => m.MapFrom(a => a.ExitProcessManager))
                    .ForMember(x => x.TimeZone,
                               m => m.MapFrom(a => a.TimeZone))
                    .ForMember(x => x.WorkLocation,
                               m => m.MapFrom(a => a.WorkLocation))
                   .ReverseMap();

                cfg.CreateMap<PersonReporting, EmployeeOrganizaionDetails>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.ReportingTo,
                               m => m.MapFrom(a => a.ReportingTo))
                    .ReverseMap()
                        .ForMember(x => x.PersonID,
                                   m => m.MapFrom(a => a.ID));

                cfg.CreateMap<PersonPassport, EmployeePassport>()
                    .ForMember(x => x.NameAsInPassport,
                               m => m.MapFrom(a => a.NameAsInPassport))
                    .ForMember(x => x.DateOfIssue,
                               m => m.MapFrom(a => a.DateOfIssue))
                    .ForMember(x => x.DateOfExpiry,
                               m => m.MapFrom(a => a.DateOfExpiry))
                    .ForMember(x => x.PlaceIssued,
                               m => m.MapFrom(a => a.PlaceIssued))
                    .ForMember(x => x.BlankPagesLeft,
                               m => m.MapFrom(a => a.BlankPagesLeft))
                    .ForMember(x => x.PassportNumber,
                               m => m.MapFrom(a => a.PassportNumber))
                    .ForMember(x => x.passportFileUrl,
                               m => m.MapFrom(a => a.PassportFileURL))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ReverseMap();


                cfg.CreateMap<PersonDeclaration, EmployeeDeclaration>()
                    .ForMember(x => x.DeclaredPerson,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.BirthDate,
                               m => m.MapFrom(a => a.BirthDate))
                    .ForMember(x => x.RelationType,
                               m => m.MapFrom(a => a.RelationType))
                    .ForMember(x => x.V2PersonID,
                               m => m.MapFrom(a => a.V2PersonID))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ReverseMap()
                        .ForMember(x => x.Name,
                                   m => m.MapFrom(a => a.DeclaredPerson));

                cfg.CreateMap<PersonDependent, EmployeeDependent>()
                    .ForMember(x => x.DependentName,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.RelationWithDependent,
                               m => m.MapFrom(a => a.Relation))
                    .ForMember(x => x.DateOfBirthOfDependent,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Age,
                               m => m.MapFrom(a =>
                                   a.DateOfBirth != null ?
                                   (System.DateTime.Today.Year - a.DateOfBirth.Value.Year) : 0))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ReverseMap()
                        .ForMember(x => x.Name,
                                   m => m.MapFrom(a => a.DependentName))
                        .ForMember(x => x.Relation,
                                   m => m.MapFrom(a => a.RelationWithDependent))
                        .ForMember(x => x.DateOfBirth,
                                   m => m.MapFrom(a => a.DateOfBirthOfDependent));

                cfg.CreateMap<PersonQualificationMapping, EmployeeQualification>()
                    .ForMember(x => x.Grade_Class,
                               m => m.MapFrom(a => a.Grade_Class))
                    .ForMember(x => x.University,
                               m => m.MapFrom(a => a.University))
                    .ForMember(x => x.QualificationID,
                               m => m.MapFrom(a => a.Qualification.ID))
                    .ForMember(x => x.Specialization,
                               m => m.MapFrom(a => a.Specialization))
                    .ForMember(x => x.PassingYear,
                               m => m.MapFrom(a => a.Year))
                    .ForMember(x => x.Institute,
                               m => m.MapFrom(a => a.Institute))
                    .ForMember(x => x.QualificationType,
                               m => m.MapFrom(a => a.QualificationType))
                    .ForMember(x => x.StatusId,
                               m => m.MapFrom(a => a.StatusId))
                    .ForMember(x => x.QualificationName,
                               m => m.MapFrom(a => a.Qualification.QualificationName))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ReverseMap()
                        .ForMember(x => x.Year,
                                   m => m.MapFrom(a => a.PassingYear));

                cfg.CreateMap<PersonSkillMapping, EmployeeSkill>()
                    .ForMember(x => x.SkillID,
                               m => m.MapFrom(a => a.SkillID))
                    .ForMember(x => x.SkillName,
                               m => m.MapFrom(a => a.SkillMatrix.Name))
                    .ForMember(x => x.SkillRating,
                               m => m.MapFrom(a => a.SkillRating))
                    .ForMember(x => x.ExperienceYears,
                               m => m.MapFrom(a => a.ExperienceYears))
                    .ForMember(x => x.ExperienceMonths,
                               m => m.MapFrom(a => a.ExperienceMonths))
                    .ForMember(x => x.HasCoreCompetency,
                               m => m.MapFrom(a => a.HasCoreCompetency))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.IsPrimary,
                               m => m.MapFrom(a => a.IsPrimary))
                    .ReverseMap();

                cfg.CreateMap<PersonEmployment, EmployeeCompetency>()
                .ForMember(x => x.CompetencyID,
                               m => m.MapFrom(a => a.CompetencyID))
                    .ForMember(x => x.SkillDescription,
                               m => m.MapFrom(a => a.SkillDescription))
                    .ReverseMap();

                cfg.CreateMap<Person, EmployeeBasicProfile>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.FirstName,
                               m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.MiddleName,
                               m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.LastName,
                               m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.CurrentDesignation,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                       a.PersonEmployment.First().Designation.Name : null))
                    .ForMember(x => x.PFNo,
                               m => m.MapFrom(a => a.PersonPersonal.PFNo))
                    .ForMember(x => x.PANNo,
                               m => m.MapFrom(a => a.PersonPersonal.PANNo))
                    .ForMember(x => x.Email,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                       a.PersonEmployment.First().OrganizationEmail : null))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.PersonPersonal.Mobile != null ?
                                       a.PersonPersonal.Mobile : null))
                    .ForMember(x => x.OL,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   a.PersonEmployment.First().OfficeLocation : 0))
                    .ForMember(x => x.OLText,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   EnumExtensions.GetEnumDescription((EnumHelpers.Location)a.PersonEmployment.FirstOrDefault().OfficeLocation) : string.Empty))
                    .ForMember(x => x.SeatingLocation,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   (a.PersonEmployment.First().SeatingLocation != null ? a.PersonEmployment.First().SeatingLocation : string.Empty) : string.Empty))
                    .ForMember(x => x.ResidenceNumber,
                               m => m.MapFrom(a => a.PersonPersonal.Phone))
                    .ForMember(x => x.ImagePath,
                               m => m.MapFrom(a => a.Image))
                    .ForMember(x => x.Active,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Extension,
                               m => m.MapFrom(a => a.PersonEmployment.Any() ?
                                   (a.PersonEmployment.First().OfficeExtension != null ? a.PersonEmployment.First().OfficeExtension : string.Empty) : string.Empty))
                    .ForMember(x => x.joiningDate,
                               m => m.MapFrom(a => a.PersonEmployment.First().JoiningDate))
                    .ForMember(x => x.probationReviewDate,
                               m => m.MapFrom(a => a.PersonEmployment.First().ProbationReviewDate))
                    .ForMember(x => x.exitDate,
                                m => m.MapFrom(a => a.PersonEmployment.First().ExitDate))
                    .ForMember(x => x.EmployeementStaus,
                                m => m.MapFrom(a => a.PersonEmployment.First().EmploymentStatus.GetValueOrDefault()))
                    .ForMember(x => x.GenderID,
                                m => m.MapFrom(a => a.Gender))
                    .ReverseMap();

                cfg.CreateMap<PersonVisa, EmployeeVisa>()
                    .ForMember(x => x.VisaTypeID,
                               m => m.MapFrom(a => a.VisaType.ID))
                    .ForMember(x => x.VisaName,
                               m => m.MapFrom(a => a.VisaType.VisaType1))
                    .ForMember(x => x.CountryID,
                               m => m.MapFrom(a => a.Country.ID))
                    .ForMember(x => x.CountryName,
                               m => m.MapFrom(a => a.Country.Name))
                    .ForMember(x => x.ValidTill,
                               m => m.MapFrom(a => a.ValidTill))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.visaFileUrl,
                               m => m.MapFrom(a => a.VisaFileURL))
                    .ReverseMap();

                cfg.CreateMap<PersonAddress, EmployeeAddress>()
                    .ForMember(x => x.CurrentAddress,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.CurrentAddressCountry,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.IsCurrent))
                    .ForMember(x => x.AddressLabel,
                               m => m.MapFrom(a =>
                                   a.IsCurrent == true ?
                                   "Current Address" : "Permanent Address"))
                    .ReverseMap()
                        .ForMember(x => x.Address,
                                   m => m.MapFrom(a => a.CurrentAddress))
                        .ForMember(x => x.Country,
                                   m => m.MapFrom(a => a.CurrentAddressCountry));

                cfg.CreateMap<PersonCertification, EmployeeCertification>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.CertificationID,
                               m => m.MapFrom(a => a.Certifications.ID))
                    .ForMember(x => x.CertificationName,
                               m => m.MapFrom(a => a.Certifications.CertificateName))
                    .ForMember(x => x.CertificationDate,
                               m => m.MapFrom(a => a.CertificationDate))
                    .ForMember(x => x.Grade,
                               m => m.MapFrom(a => a.Grade))
                    .ForMember(x => x.StatusID,
                               m => m.MapFrom(a => a.StatusId))
                    .ForMember(x => x.CertificationNumber,
                               m => m.MapFrom(a => a.CertificationNumber))
                    .ReverseMap();

                cfg.CreateMap<PersonEmploymentHistory, EmployeeEmploymentHistory>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.OrganisationName,
                               m => m.MapFrom(a => a.OrganisationName))
                    .ForMember(x => x.Location,
                               m => m.MapFrom(a => a.Location))
                    .ForMember(x => x.JoiningDate,
                               m => m.MapFrom(a => a.JoiningDate))
                    .ForMember(x => x.WorkedTill,
                               m => m.MapFrom(a => a.WorkedTill))
                    .ForMember(x => x.LastDesignation,
                               m => m.MapFrom(a => a.LastDesignation))
                    .ReverseMap();

                cfg.CreateMap<PersonPastExperience, EmployeePreviousExperience>()

                    .ForMember(x => x.year,
                               m => m.MapFrom(a => a.ExperienceYears))
                    .ForMember(x => x.month,
                               m => m.MapFrom(a => a.ExperienceMonths))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ReverseMap();

                cfg.CreateMap<PersonMedicalHistory, EmployeeMedicalHistory>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Description,
                               m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.Year,
                               m => m.MapFrom(a => a.Year))
                    .ReverseMap();

                cfg.CreateMap<Module, UserMenuViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.IsActive,
                               m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.ParentID,
                               m => m.MapFrom(a => a.ParentID))
                    .ForMember(x => x.Action,
                               m => m.MapFrom(a => a.Action))
                    .ForMember(x => x.IsPageLevelSubMenu,
                               m => m.MapFrom(a => a.IsPageLevelSubMenu))
                    .ForMember(x => x.UseAsDefault,
                               m => m.MapFrom(a => a.UseAsDefault))
                    .ForMember(x => x.ImageUrl,
                               m => m.Ignore())
                    .ForMember(x => x.IconCSS,
                               m => m.Ignore())
                    .ForMember(x => x.CSS,
                               m => m.Ignore())
                    .ReverseMap();

                cfg.CreateMap<EmployeePersonalDetails, EmployeeProfileViewModel>()
                    .ForMember(x => x.UserName,
                               m => m.MapFrom(a => a.UserName))
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a => a.DateOfBirth))
                    .ForMember(x => x.Gender,
                               m => m.MapFrom(a => a.Gender))
                    .ForMember(x => x.MaritalStatus,
                               m => m.MapFrom(a => a.MaritalStatus))
                    .ForMember(x => x.WeddingDate,
                               m => m.MapFrom(a => a.WeddingDate))
                    .ForMember(x => x.SpouseName,
                               m => m.MapFrom(a => a.SpouseName))
                    .ForMember(x => x.SpouseBirthDate,
                               m => m.MapFrom(a => a.SpouseBirthDate))
                    .ForMember(x => x.NoofChildren,
                               m => m.MapFrom(a => a.NoofChildren))
                    .ForMember(x => x.Hobbies,
                               m => m.MapFrom(a => a.Hobbies))
                    .ForMember(x => x.FirstChildName,
                               m => m.MapFrom(a => a.FirstChildName))
                    .ForMember(x => x.FirstChildDateOfBirth,
                               m => m.MapFrom(a => a.FirstChildDateOfBirth))
                    .ReverseMap();

                cfg.CreateMap<EmployeeProfileViewModel, PersonPersonal>()
                    .ForMember(x => x.MaritalStatus,
                               m => m.MapFrom(a => a.MaritalStatus))
                    .ForMember(x => x.DateOfMarriage,
                               m => m.MapFrom(a => a.WeddingDate))
                    .ForMember(x => x.SpouseName,
                               m => m.MapFrom(a => a.SpouseName))
                    .ForMember(x => x.SpouseBirthDate,
                               m => m.MapFrom(a => a.SpouseBirthDate))
                    .ForMember(x => x.NoofChildren,
                               m => m.MapFrom(a => a.NoofChildren))
                    .ForMember(x => x.Hobbies,
                               m => m.MapFrom(a => a.Hobbies))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Phone,
                               m => m.MapFrom(a => a.ResidenceNumber))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.PersonalEmail,
                               m => m.MapFrom(a => a.PersonalEmail))
                    .ForMember(x => x.BloodGroup,
                               m => m.MapFrom(a => a.bloodGroup))
                    .ForMember(x => x.PANNo,
                               m => m.MapFrom(a => a.PANNo))
                    .ReverseMap();

                cfg.CreateMap<EmployeeProfileViewModel, PersonEmployment>()
                    .ForMember(x => x.UserName,
                               m => m.MapFrom(a => a.UserName))
                    .ForMember(x => x.JoiningDate,
                               m => m.MapFrom(a => a.JoiningDate))
                    .ForMember(x => x.DesignationID,
                               m => m.MapFrom(a => a.CurrentDesignationID))
                    .ForMember(x => x.ProbationReviewDate,
                               m => m.MapFrom(a => a.ProbationReviewDate))
                    .ForMember(x => x.ConfirmationDate,
                               m => m.MapFrom(a => a.ConfirmationDate))
                    .ForMember(x => x.ExitDate,
                               m => m.MapFrom(a => a.ExitDate))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.ID,
                               m => m.Ignore())
                     .ForMember(x => x.CompetencyID,
                               m => m.MapFrom(a => a.CompetencyID))
                    .ForMember(x => x.SkillDescription,
                               m => m.MapFrom(a => a.SkillDescription))
                    .ReverseMap();

                cfg.CreateMap<EmployeeProfileViewModel, PersonAddress>()
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Addresses.First().CurrentAddress))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Addresses.First().CurrentAddressCountry))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.Addresses.First().IsCurrent))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.ID))
                    .ReverseMap();

                cfg.CreateMap<EmployeeProfileViewModel, PersonDependent>()
                    .ForMember(x => x.DateOfBirth,
                               m => m.MapFrom(a =>
                                   a.Dependents.Where(u => u.RelationWithDependent == 6 || u.RelationWithDependent == 7).Any() ?
                                   a.Dependents.First().DateOfBirthOfDependent : null))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a =>
                                   a.Dependents.Where(u => u.RelationWithDependent == 6 || u.RelationWithDependent == 7).Any() ?
                                   a.Dependents.First().DependentName : null))
                    .ReverseMap();

                cfg.CreateMap<EmployeeProfileViewModel, EmployeeAddressViewModel>()
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Addresses.First().CurrentAddress))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Addresses.First().CurrentAddressCountry))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.Addresses.First().IsCurrent))
                    .ForMember(x => x.Phone,
                               m => m.MapFrom(a => a.ResidenceNumber))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.PersonalEmail,
                               m => m.MapFrom(a => a.PersonalEmail))
                    .ReverseMap();

                cfg.CreateMap<EmployeeAddressViewModel, PersonAddress>()
                    .ForMember(x => x.Address,
                               m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.Country,
                               m => m.MapFrom(a => a.Country))
                    .ForMember(x => x.IsCurrent,
                               m => m.MapFrom(a => a.IsCurrent))
                    .ReverseMap();

                cfg.CreateMap<EmployeePassportViewModel, PersonPassport>()
                    .ForMember(x => x.NameAsInPassport,
                               m => m.MapFrom(a => a.NameAsInPassport))
                    .ForMember(x => x.DateOfIssue,
                               m => m.MapFrom(a => a.DateOfIssue))
                    .ForMember(x => x.DateOfExpiry,
                               m => m.MapFrom(a => a.DateOfExpiry))
                    .ForMember(x => x.PlaceIssued,
                               m => m.MapFrom(a => a.PlaceIssued))
                    .ForMember(x => x.BlankPagesLeft,
                               m => m.MapFrom(a => a.BlankPagesLeft))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PassportNumber,
                               m => m.MapFrom(a => a.PassportNumber))
                    .ForMember(x => x.PassportFileURL,
                               m => m.MapFrom(a => a.passportFileUrl))
                    .ReverseMap();

                cfg.CreateMap<SignInSignOut, EmployeeSISOViewModel>()
                  .ForMember(x => x.Id,
                            m => m.MapFrom(a => a.SignInSignOutID))
                  .ForMember(x => x.UserID,
                            m => m.MapFrom(a => a.UserID))
                  .ForMember(x => x.Date,
                            m => m.MapFrom(a => a.SignInTime.HasValue ? ((System.DateTime)a.SignInTime).ToShortDateString() : a.SignOutTime.HasValue ? ((System.DateTime)a.SignOutTime).ToShortDateString() : ""))
                 .ForMember(x => x.SignInTime,
                            m => m.MapFrom(a => a.SignInTime.HasValue ? new System.TimeSpan(a.SignInTime.Value.TimeOfDay.Hours, a.SignInTime.Value.TimeOfDay.Minutes, 0) : System.TimeSpan.Zero))
                 .ForMember(x => x.SignOutTime,
                            m => m.MapFrom(a => a.SignOutTime.HasValue ? new System.TimeSpan(a.SignOutTime.Value.TimeOfDay.Hours, a.SignOutTime.Value.TimeOfDay.Minutes, 0) : System.TimeSpan.Zero))
                 .ForMember(x => x.TotalHoursWorked,
                            m => m.MapFrom(a => a.TotalHoursWorked == null ? "0:0" : a.TotalHoursWorked))//  a.SignOutTime.HasValue ? new System.TimeSpan(a.SignOutTime.Value.TimeOfDay.Hours, a.SignOutTime.Value.TimeOfDay.Minutes, 0).Subtract(new System.TimeSpan(a.SignInTime.Value.TimeOfDay.Hours, a.SignInTime.Value.TimeOfDay.Minutes, 0)).ToString() : System.TimeSpan.Zero.ToString()))
                     .ForMember(x => x.Narration,
                                m => m.MapFrom(a => string.Format(a.SignInComment + " | " + a.SignOutComment)))
                     .ForMember(x => x.IsManual,
                                m => m.MapFrom(a => a.IsSignInManual))
                     .ForMember(x => x.StatusID,
                                m => m.MapFrom(a => a.statusID))
                     .ForMember(x => x.ApproverComments,
                                m => m.MapFrom(a => a.ApproverComments))
                     .ReverseMap();

                cfg.CreateMap<SISOManualAutoViewModel, SignInSignOut>()
                  .ForMember(x => x.SignInTime, m => m.MapFrom(a => a.IsSignIn ? new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(a.Time) : System.DateTime.MinValue))
                   .ForMember(x => x.SignOutTime, m => m.MapFrom(a => (!a.IsSignIn) ? new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(a.Time) : System.DateTime.MinValue))
                   .ForMember(x => x.LastModified, m => m.MapFrom(a => System.DateTime.Now))
                   .ForMember(x => x.IsSignInManual, m => m.MapFrom(a => a.IsManual))
                   .ForMember(x => x.SignInComment, m => m.MapFrom(a => a.IsSignIn ? a.Narration : System.String.Empty))
                   .ForMember(x => x.SignOutComment, m => m.MapFrom(a => (!a.IsSignIn) ? a.Narration : System.String.Empty))
                   .ForMember(x => x.IsSignOutManual, m => m.MapFrom(a => a.IsManual))
                   .ForMember(x => x.TimeZoneName, m => m.MapFrom(a => a.TimeZoneName))
                   .ReverseMap();

                cfg.CreateMap<PersonContact, EmployeeEmergencyContact>()
                  .ForMember(x => x.ContactPersonName,
                             m => m.MapFrom(a => a.Name))
                  .ForMember(x => x.EmergencyContactNo,
                             m => m.MapFrom(a => a.Mobile))
                  .ForMember(x => x.Relation,
                             m => m.MapFrom(a => a.Relation))
                  .ForMember(x => x.EmergencyEmail,
                            m => m.MapFrom(a => a.Email))
                  .ForMember(x => x.ID,
                            m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.ContactAddress,
                               m => m.MapFrom(a => a.Address))
                  .ReverseMap()
                      .ForMember(x => x.Name,
                                 m => m.MapFrom(a => a.ContactPersonName))
                      .ForMember(x => x.Mobile,
                                 m => m.MapFrom(a => a.EmergencyContactNo))
                      .ForMember(x => x.Email,
                                m => m.MapFrom(a => a.EmergencyEmail))
                      .ForMember(x => x.Address,
                                m => m.MapFrom(a => a.ContactAddress));

                cfg.CreateMap<Expense, ExpenseViewModel>()
                    .ForMember(x => x.IsClientReimbursment, m => m.MapFrom(a => a.IsClientReimbursment))
                    .ForMember(x => x.expenseId, m => m.MapFrom(a => a.ExpenseId))
                    .ForMember(x => x.details, m => m.MapFrom(a => a.ExpenseDetails))
                    .ForMember(x => x.ReimbursmentTitle, m => m.MapFrom(a => a.ReimbursementTitle))
                    .ForMember(x => x.ClientId, m => m.MapFrom(a => a.ClientName))
                    .ForMember(x => x.CurrencyId, m => m.MapFrom(a => a.Currency))
                    .ForMember(x => x.CostCenterId, m => m.MapFrom(a => a.CostCenter))
                    .ForMember(x => x.PrimaryApproverId, m => m.MapFrom(a => a.PrimaryApprover))
                    .ForMember(x => x.SecondaryApproverId, m => m.MapFrom(a => a.SecondaryApprover))
                    .ForMember(x => x.IsApproved, m => m.MapFrom(a => a.IsApproved))
                    .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.Person.ID))
                    .ForMember(x => x.FirstName, m => m.MapFrom(a => a.Person.FirstName.ToString()))
                    .ForMember(x => x.LastName, m => m.MapFrom(a => a.Person.LastName))
                    .ForMember(x => x.IsRejected, m => m.MapFrom(a => a.IsRejected))
                    .ForMember(x => x.RequestDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.advance, m => m.MapFrom(a => a.Advance))
                    .ForMember(x => x.amountReimbursed, m => m.MapFrom(a => a.Balance))
                    .ForMember(x => x.totalExpenses, m => m.MapFrom(a => a.TotalAmount))
                    .ForMember(x => x.stageId, m => m.MapFrom(a => a.StageID))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                    //.ForMember(x => x.officeLocation, m => m.MapFrom(a => a.Person.PersonEmployment.First().OfficeLocation))
                    .ReverseMap()
                    .ForMember(x => x.ClientName, m => m.MapFrom(a => a.ClientId))
                    .ForMember(x => x.Currency, m => m.MapFrom(a => a.CurrencyId))
                    .ForMember(x => x.CostCenter, m => m.MapFrom(a => a.CostCenterId))
                    .ForMember(x => x.PrimaryApprover, m => m.MapFrom(a => a.PrimaryApproverId))
                    .ForMember(x => x.SecondaryApprover, m => m.MapFrom(a => a.SecondaryApproverId))
                    .ForMember(x => x.ExpenseDetails, m => m.MapFrom(a => a.details))
                    .ForMember(x => x.ReimbursementTitle, m => m.MapFrom(a => a.ReimbursmentTitle))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.RequestDate))
                    .ForMember(x => x.Advance, m => m.MapFrom(a => a.advance))
                    .ForMember(x => x.Balance, m => m.MapFrom(a => a.amountReimbursed))
                    .ForMember(x => x.TotalAmount, m => m.MapFrom(a => a.totalExpenses));

                cfg.CreateMap<ExpenseDetails, EmployeeExpenseDetails>()
                     .ForMember(x => x.Amount, m => m.MapFrom(a => a.Amount))
                     .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile))
                     .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                     .ForMember(x => x.ExpenseCategoryId, m => m.MapFrom(a => a.ExpenseCategoryId))
                     .ForMember(x => x.ExpenseDate, m => m.MapFrom(a => a.ExpenseDate))
                     .ForMember(x => x.ReceiptNo, m => m.MapFrom(a => a.ReceiptNo))
                     .ForMember(x => x.ExpenseDetailId, m => m.MapFrom(a => a.ExpenseDetailId))
                     .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile)).ReverseMap();

                cfg.CreateMap<PersonLeave, EmployeeLeaveViewModel>()
                 .ForMember(x => x.FromDate,
                            m => m.MapFrom(a => a.FromDate))
                 .ForMember(x => x.ToDate,
                            m => m.MapFrom(a => a.ToDate))
                 .ForMember(x => x.Narration,
                            m => m.MapFrom(a => a.Narration))
                 .ForMember(x => x.ID,
                           m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Leaves,
                           m => m.MapFrom(a => a.Leaves))
                 .ForMember(x => x.LeaveType,
                           m => m.MapFrom(a => a.LeaveType))
                 .ForMember(x => x.Status,
                           m => m.MapFrom(a => a.Status))
                 .ForMember(x => x.Absent,
                           m => m.MapFrom(a => a.Status == 3 ? a.ToDate.Subtract(a.FromDate.Date).Days + 1 : 0))
                 .ReverseMap();

                cfg.CreateMap<PersonLeave, ApprovalLeaveViewModel>()
                 .ForMember(x => x.FromDate,
                            m => m.MapFrom(a => a.FromDate))
                 .ForMember(x => x.ToDate,
                            m => m.MapFrom(a => a.ToDate))
                 .ForMember(x => x.Narration,
                            m => m.MapFrom(a => a.Narration))
                 .ForMember(x => x.ID,
                           m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Leaves,
                           m => m.MapFrom(a => a.Leaves))
                 .ForMember(x => x.LeaveType,
                           m => m.MapFrom(a => a.LeaveType))
                 .ForMember(x => x.Status,
                           m => m.MapFrom(a => a.Status))
                 .ForMember(x => x.Absent,
                           m => m.MapFrom(a => a.Status == 3 ? a.ToDate.Subtract(a.FromDate.Date).Days + 1 : 0))
                 .ForMember(x => x.UserId,
                           m => m.MapFrom(a => a.Person.ID))
                 .ForMember(x => x.UserName,
                           m => m.MapFrom(a => a.Person.FirstName + " " + a.Person.MiddleName + " " + a.Person.LastName))
                 .ForMember(x => x.ImagePath,
                           m => m.MapFrom(a => a.Person.Image))
                 .ForMember(x => x.CurrentDesignation,
                           m => m.MapFrom(a => a.Person.PersonEmployment.First().Designation.Name))
                 .ForMember(x => x.OLText,
                           m => m.MapFrom(a => a.Person.PersonEmployment.Any() ? EnumExtensions.GetEnumDescription((EnumHelpers.Location)a.Person.PersonEmployment.First().OfficeLocation) : string.Empty))
                 .ForMember(x => x.Email,
                           m => m.MapFrom(a => a.Person.PersonEmployment.First().OrganizationEmail))
                 .ForMember(x => x.Mobile,
                           m => m.MapFrom(a => a.Person.PersonPersonal.Mobile))
                 .ForMember(x => x.ResidenceNumber,
                           m => m.MapFrom(a => a.Person.PersonPersonal.Phone))
                 .ReverseMap();

                cfg.CreateMap<CompOff, ApprovalCompOffViewModel>()
                    .ForMember(x => x.ForDate,
                               m => m.MapFrom(a => a.ForDate))
                    .ForMember(x => x.ExpiresOn,
                               m => m.MapFrom(a => a.ExpiresOn))
                    .ForMember(x => x.IsApplied,
                               m => m.MapFrom(a => a.IsApplied))
                    .ForMember(x => x.Status,
                               m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.UserId,
                               m => m.MapFrom(a => a.Person.ID))
                    .ForMember(x => x.UserName,
                               m => m.MapFrom(a => a.Person.FirstName + " " + a.Person.MiddleName + " " + a.Person.LastName))
                    .ForMember(x => x.ImagePath,
                               m => m.MapFrom(a => a.Person.Image))
                    .ForMember(x => x.CurrentDesignation,
                               m => m.MapFrom(a => a.Person.PersonEmployment.First().Designation.Name))
                    .ForMember(x => x.OLText,
                               m => m.MapFrom(a => a.Person.PersonEmployment.Any() ? EnumExtensions.GetEnumDescription((EnumHelpers.Location)a.Person.PersonEmployment.First().OfficeLocation) : string.Empty))
                    .ForMember(x => x.Email,
                               m => m.MapFrom(a => a.Person.PersonEmployment.First().OrganizationEmail))
                    .ForMember(x => x.Mobile,
                               m => m.MapFrom(a => a.Person.PersonPersonal.Mobile))
                    .ForMember(x => x.ResidenceNumber,
                               m => m.MapFrom(a => a.Person.PersonPersonal.Phone))
                    .ReverseMap();

                cfg.CreateMap<PersonLeaveLedger, AvailableLeaves>()
                 .ForMember(x => x.TotalLeaves,
                           m => m.MapFrom(a => a.OpeningBalance))
                .ForMember(x => x.LeavesTaken,
                           m => m.MapFrom(a => a.LeaveUtilized))
                 .ReverseMap();

                cfg.CreateMap<HolidayList, HolidayListViewModel>()
                 .ForMember(x => x.HolidayDate,
                            m => m.MapFrom(a => a.Date))
                 .ForMember(x => x.Description,
                            m => m.MapFrom(a => a.Description.Trim()))
                 .ReverseMap();

                cfg.CreateMap<Stage, EmployeeApprovalViewModel>()
                 .ForMember(x => x.OldModel,
                            m => m.MapFrom(a => a.PreviousEntry))
                 .ForMember(x => x.NewModel,
                            m => m.MapFrom(a => a.NewEntry))
                 .ForMember(x => x.StageID,
                            m => m.MapFrom(a => "stg_" + a.ID.ToString()))
                 .ForMember(x => x.ModuleID,
                            m => m.MapFrom(a => a.ModuleID))
                 .ForMember(x => x.ModuleCode,
                            m => m.MapFrom(a => a.ModuleCode))
                 .ForMember(x => x.By,
                            m => m.MapFrom(a => a.By))
                 .ForMember(x => x.ApprovalStatus,
                            m => m.MapFrom(a => a.ApprovalStatus))
                 .ForMember(x => x.Comments,
                            m => m.MapFrom(a => a.Comments))
                 .ForMember(x => x.ApprovedBy,
                            m => m.MapFrom(a => a.ApprovedBy))
                 .ForMember(x => x.ApprovedDate,
                            m => m.MapFrom(a => a.ApprovedDate))
                 .ReverseMap()
                     .ForMember(x => x.PreviousEntry,
                                m => m.MapFrom(a => a.OldModel))
                     .ForMember(x => x.NewEntry,
                                m => m.MapFrom(a => a.NewModel));

                cfg.CreateMap<MultiRecordStage, EmployeeApprovalViewModel>()
                 .ForMember(x => x.OldModel,
                            m => m.MapFrom(a => a.PreviousEntry))
                 .ForMember(x => x.NewModel,
                            m => m.MapFrom(a => a.NewEntry))
                 .ForMember(x => x.StageID,
                            m => m.MapFrom(a => "mrstg_" + a.ID.ToString()))
                 .ForMember(x => x.ModuleID,
                            m => m.MapFrom(a => a.ModuleID))
                 .ForMember(x => x.ModuleCode,
                            m => m.MapFrom(a => a.ModuleCode))
                 .ForMember(x => x.By,
                            m => m.MapFrom(a => a.By))
                 .ForMember(x => x.ApprovalStatus,
                            m => m.MapFrom(a => a.ApprovalStatus))
                 .ForMember(x => x.Comments,
                            m => m.MapFrom(a => a.Comments))
                 .ForMember(x => x.ApprovedBy,
                            m => m.MapFrom(a => a.ApprovedBy))
                 .ForMember(x => x.ApprovedDate,
                            m => m.MapFrom(a => a.ApprovedDate))
                 .ForMember(x => x.RecordID,
                            m => m.MapFrom(a => a.RecordID))
                 .ReverseMap()
                     .ForMember(x => x.PreviousEntry,
                                m => m.MapFrom(a => a.OldModel))
                     .ForMember(x => x.NewEntry,
                                m => m.MapFrom(a => a.NewModel));

                cfg.CreateMap<PersonInRole, EmployeeRole>()
                    .ForMember(x => x.role, m => m.MapFrom(a => a.Role.Name))
                    .ForMember(x => x.roleId, m => m.MapFrom(a => a.RoleID));

                cfg.CreateMap<PersonHelpDesk, HelpDeskModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.AssignedTo,
                              m => m.MapFrom(a => a.AssignedTo))
                   .ForMember(x => x.CategoryID,
                              m => m.MapFrom(a => a.CategoryID))
                   .ForMember(x => x.IsApprovalRequired,
                              m => m.MapFrom(a => a.IsApprovalRequired))
                   .ForMember(x => x.IssueDate,
                              m => m.MapFrom(a => a.IssueDate))
                   .ForMember(x => x.PersonID,
                              m => m.MapFrom(a => a.PersonID))
                   .ForMember(x => x.Severity,
                              m => m.MapFrom(a => a.Severity))
                   .ForMember(x => x.Status,
                              m => m.MapFrom(a => a.Status))
                   .ForMember(x => x.SubCategoryID,
                              m => m.MapFrom(a => a.SubCategoryID))
                   .ForMember(x => x.RequiredTill,
                              m => m.MapFrom(a => a.RequiredCompletionDate))
                   .ForMember(x => x.PokedDate,
                               m => m.MapFrom(a => a.PokedDate));

                cfg.CreateMap<HelpDeskModel, PersonHelpDesk>()
                  .ForMember(x => x.ID,
                             m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.AssignedTo,
                             m => m.MapFrom(a => a.AssignedTo))
                  .ForMember(x => x.CategoryID,
                             m => m.MapFrom(a => a.CategoryID))
                  .ForMember(x => x.IsApprovalRequired,
                             m => m.MapFrom(a => a.IsApprovalRequired))
                  .ForMember(x => x.IssueDate,
                             m => m.MapFrom(a => a.IssueDate))
                  .ForMember(x => x.PersonID,
                             m => m.MapFrom(a => a.PersonID))
                  .ForMember(x => x.Severity,
                             m => m.MapFrom(a => a.Severity))
                  .ForMember(x => x.Status,
                             m => m.MapFrom(a => a.Status))
                  .ForMember(x => x.SubCategoryID,
                             m => m.MapFrom(a => a.SubCategoryID))
                  .ForMember(x => x.RequiredCompletionDate,
                              m => m.MapFrom(a => a.RequiredTill))
                  .ForMember(x => x.PokedDate,
                               m => m.MapFrom(a => a.PokedDate));

                cfg.CreateMap<HelpDeskComments, HelpDeskCommentModel>()
                .ForMember(x => x.ID,
                        m => m.MapFrom(a => a.ID))
                .ForMember(x => x.CommentedBy,
                        m => m.MapFrom(a => a.CommentedBy))
                .ForMember(x => x.CommentedDate,
                        m => m.MapFrom(a => a.CommentedDate))
                .ForMember(x => x.Comments,
                        m => m.MapFrom(a => a.Comments))
                .ForMember(x => x.PersonHelpDeskID,
                        m => m.MapFrom(a => a.PersonHelpDeskID))
                .ForMember(x => x.CommentedByName,
                        m => m.MapFrom(a => a.Person.FirstName + " " + a.Person.MiddleName + " " + a.Person.LastName))
                .ForMember(x => x.AttachedFile,
                    m => m.MapFrom(a => a.AttachedFile))
                .ForMember(x => x.CommentedByRole,
                        m => m.MapFrom(a => a.Person.PersonEmployment.FirstOrDefault().Designation.Description));

                cfg.CreateMap<HelpDeskCommentModel, HelpDeskComments>()
                .ForMember(x => x.ID,
                            m => m.MapFrom(a => a.ID))
                .ForMember(x => x.CommentedBy,
                            m => m.MapFrom(a => a.CommentedBy))
                .ForMember(x => x.CommentedDate,
                            m => m.MapFrom(a => a.CommentedDate))
                .ForMember(x => x.Comments,
                            m => m.MapFrom(a => a.Comments))
                .ForMember(x => x.PersonHelpDeskID,
                            m => m.MapFrom(a => a.PersonHelpDeskID))
                .ForMember(x => x.PersonHelpDeskID,
                        m => m.MapFrom(a => a.PersonHelpDeskID));

                cfg.CreateMap<PersonHelpDesk, HelpDeskListModel>()
                .ForMember(x => x.ID,
                           m => m.MapFrom(a => a.ID))
                .ForMember(x => x.AssignedTo,
                           m => m.MapFrom(a => a.AssignedTo))
                .ForMember(x => x.IssueDate,
                           m => m.MapFrom(a => a.IssueDate))
               .ForMember(x => x.Severity,
                            m => m.MapFrom(a => Enum.GetName(typeof(HelpDeskSeverity), a.Severity)))
                .ForMember(x => x.Type,
                            m => m.MapFrom(a => Enum.GetName(typeof(HelpDeskType), a.Type)))
                .ForMember(x => x.Status,
                           m => m.MapFrom(a => a.Status))
                .ForMember(x => x.AssignedToName,
                            m => m.MapFrom(a => a.Person.FirstName + " " + a.Person.MiddleName + " " + a.Person.LastName))
                .ForMember(x => x.Number,
                           m => m.MapFrom(a => a.number))
                .ForMember(x => x.RequestedBy,
                           m => m.MapFrom(a => a.Person1.FirstName + " " + a.Person1.MiddleName + " " + a.Person1.LastName))
                .ForMember(x => x.CategoryId,
                           m => m.MapFrom(a => a.CategoryID));

                cfg.CreateMap<PersonHelpDesk, HelpDeskReadOnlyModel>()
                .ForMember(x => x.ID,
                           m => m.MapFrom(a => a.ID))
                .ForMember(x => x.Severity,
                            m => m.MapFrom(a => Enum.GetName(typeof(HelpDeskSeverity), a.Severity)))
                .ForMember(x => x.Type,
                            m => m.MapFrom(a => Enum.GetName(typeof(HelpDeskType), a.Type)))
                .ForMember(x => x.Category,
                           m => m.MapFrom(a => a.HelpDeskCategories.Name))
                .ForMember(x => x.SubCategory,
                            m => m.MapFrom(a => a.HelpDeskSubCategories.Name))
                .ForMember(x => x.Status,
                            m => m.MapFrom(a => a.Status))
                 .ForMember(x => x.Duration,
                            m => m.MapFrom(a => a.Duration))
                 .ForMember(x => x.Number,
                           m => m.MapFrom(a => a.number))
                 .ForMember(x => x.RequiredTill,
                           m => m.MapFrom(a => a.RequiredCompletionDate))
                 .ForMember(x => x.PokedDate,
                           m => m.MapFrom(a => a.PokedDate))
                 .ForMember(x => x.AssignedTo,
                           m => m.MapFrom(a => a.AssignedTo));

                cfg.CreateMap<Traveller, TravelViewModel>()
                    .ForMember(x => x.clientInformation, m => m.MapFrom(a => a.ClientInformation))
                    .ForMember(x => x.nomineeDetails, m => m.MapFrom(a => a.NomineeDetails))
                    .ForMember(x => x.travelDetails, m => m.MapFrom(a => a.TravelDetails))
                    .ForMember(x => x.primaryApproverId, m => m.MapFrom(a => a.PrimaryApproverId.Value))
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.hotelBooking, m => m.MapFrom(a => a.HotelBooking))
                    .ForMember(x => x.travelExtension, m => m.MapFrom(a => a.TravelDetails.TravelExtensionHistory))
                    .ReverseMap()
                    .ForMember(x => x.ClientInformation, m => m.MapFrom(a => a.clientInformation))
                    .ForMember(x => x.NomineeDetails, m => m.MapFrom(a => a.nomineeDetails))
                    .ForMember(x => x.TravelDetails, m => m.MapFrom(a => a.travelDetails))
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.HotelBooking, m => m.MapFrom(a => a.hotelBooking));

                cfg.CreateMap<ClientInformation, ClientInformationVM>()
                    .ForMember(x => x.adddress, m => m.MapFrom(a => a.Address))
                    .ForMember(x => x.purposeOfVisit, m => m.MapFrom(a => a.PurposeOfVisit))
                    .ForMember(x => x.clientName, m => m.MapFrom(a => a.ClientName))
                    .ForMember(x => x.description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.clientId, m => m.MapFrom(a => a.ClientID))
                    .ReverseMap()
                    .ForMember(x => x.Address, m => m.MapFrom(a => a.adddress))
                    .ForMember(x => x.PurposeOfVisit, m => m.MapFrom(a => a.purposeOfVisit))
                    .ForMember(x => x.ClientName, m => m.MapFrom(a => a.clientName))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.description));

                cfg.CreateMap<NomineeDetails, NomineeDetailsVM>()
                   .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.ContactNumber, m => m.MapFrom(a => a.ContactNumber))
                   .ForMember(x => x.Relationship, m => m.MapFrom(a => a.Relationship))
                   .ForMember(x => x.Address, m => m.MapFrom(a => a.Address))
                   .ReverseMap()
                   .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.ContactNumber, m => m.MapFrom(a => a.ContactNumber))
                   .ForMember(x => x.Relationship, m => m.MapFrom(a => a.Relationship))
                   .ForMember(x => x.Address, m => m.MapFrom(a => a.Address));

                cfg.CreateMap<TravelDetails, TravelDetailsVM>()
                    .ForMember(x => x.returnJourneyDate, m => m.MapFrom(a => a.ReturnJourneyDate))
                    .ForMember(x => x.journeyDate, m => m.MapFrom(a => a.JourneyDate))
                    .ForMember(x => x.source, m => m.MapFrom(a => a.Source))
                    .ForMember(x => x.destination, m => m.MapFrom(a => a.Destination))
                    .ForMember(x => x.travelType, m => m.MapFrom(a => a.TravelType))
                    .ForMember(x => x.tripType, m => m.MapFrom(a => a.TripType))
                    .ForMember(x => x.reportingManagerId, m => m.MapFrom(a => a.ReportingManagerId))
                    .ForMember(x => x.exitManagerId, m => m.MapFrom(a => a.ExitManagerId))
                    .ForMember(x => x.projectName, m => m.MapFrom(a => a.ProjectName))
                    .ForMember(x => x.requestType, m => m.MapFrom(a => a.RequestType))
                    .ForMember(x => x.accommodation, m => m.MapFrom(a => a.Accommodation))
                    .ForMember(x => x.checkInDate, m => m.MapFrom(a => a.CheckInDate))
                    .ForMember(x => x.checkOutDate, m => m.MapFrom(a => a.CheckOutDate))
                    .ReverseMap()
                    .ForMember(x => x.ReturnJourneyDate, m => m.MapFrom(a => a.returnJourneyDate))
                    .ForMember(x => x.JourneyDate, m => m.MapFrom(a => a.journeyDate))
                    .ForMember(x => x.Source, m => m.MapFrom(a => a.source))
                    .ForMember(x => x.Destination, m => m.MapFrom(a => a.destination))
                    .ForMember(x => x.TravelType, m => m.MapFrom(a => a.travelType))
                    .ForMember(x => x.TripType, m => m.MapFrom(a => a.tripType))
                    .ForMember(x => x.ReportingManagerId, m => m.MapFrom(a => a.reportingManagerId))
                    .ForMember(x => x.ExitManagerId, m => m.MapFrom(a => a.exitManagerId))
                    .ForMember(x => x.ProjectName, m => m.MapFrom(a => a.projectName))
                    .ForMember(x => x.RequestType, m => m.MapFrom(a => a.requestType))
                    .ForMember(x => x.Accommodation, m => m.MapFrom(a => a.accommodation))
                    .ForMember(x => x.CheckInDate, m => m.MapFrom(a => a.checkInDate))
                    .ForMember(x => x.CheckOutDate, m => m.MapFrom(a => a.checkOutDate));

                cfg.CreateMap<GetDetailsToApproveTravelRequest_Result, TravelListViewModel>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.primaryApproverId, m => m.MapFrom(a => a.PrimaryApproverID))
                    .ForMember(x => x.createdDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.primaryApproverName, m => m.MapFrom(a => a.PrimaryApproverName))
                    .ForMember(x => x.adminApproverName, m => m.MapFrom(a => a.AdminApproverName))
                    .ForMember(x => x.IsTravelExtensionHistoryExisting, m => m.MapFrom(a => a.IsTravelExtensionHistoryExisting));

                cfg.CreateMap<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.EmployeeProfile>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.firstName, m => m.MapFrom(a => a.FirstName))
                    .ForMember(x => x.middleName, m => m.MapFrom(a => a.MiddleName))
                    .ForMember(x => x.lastName, m => m.MapFrom(a => a.LastName))
                    .ForMember(x => x.currentDesignation, m => m.MapFrom(a => a.currentDesignation))
                    .ForMember(x => x.imagePath, m => m.MapFrom(a => a.ImagePath))
                    .ForMember(x => x.email, m => m.MapFrom(a => a.Email))
                    .ForMember(x => x.mobile, m => m.MapFrom(a => a.Mobile))
                    .ForMember(x => x.olText, m => m.MapFrom(a => a.olText));

                cfg.CreateMap<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.ClientInformation>()
                    .ForMember(x => x.clientName, m => m.MapFrom(a => a.clientName));

                cfg.CreateMap<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.TravelDetails>()
                    .ForMember(x => x.source, m => m.MapFrom(a => a.Source))
                    .ForMember(x => x.destination, m => m.MapFrom(a => a.Destination))
                    .ForMember(x => x.journeyDate, m => m.MapFrom(a => a.JourneyDate))
                    .ForMember(x => x.returnJourneyDate, m => m.MapFrom(a => a.ReturnJourneyDate))
                    .ForMember(x => x.requestType, m => m.MapFrom(a => a.RequestType))
                    .ForMember(x => x.tripType, m => m.MapFrom(a => a.TripType));

                cfg.CreateMap<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.TravelStatus>()
                    .ForMember(x => x.stage1_approverID, m => m.MapFrom(a => a.Stage1_ApproverID))
                    .ForMember(x => x.stage1_stage, m => m.MapFrom(a => a.Stage1_Stage))
                    .ForMember(x => x.stage1_status, m => m.MapFrom(a => a.Stage1_Status))
                    .ForMember(x => x.stage1_statusComment, m => m.MapFrom(a => a.Stage1_StatusComment))
                    .ForMember(x => x.stage2_approverID, m => m.MapFrom(a => a.Stage2_ApproverID))
                    .ForMember(x => x.stage2_stage, m => m.MapFrom(a => a.Stage2_Stage))
                    .ForMember(x => x.stage2_status, m => m.MapFrom(a => a.Stage2_Status))
                    .ForMember(x => x.stage2_statusComment, m => m.MapFrom(a => a.Stage2_StatusComment));

                cfg.CreateMap<ClientName, ClientNameVM>()
                    .ForMember(x => x.clientId, m => m.MapFrom(a => a.ClientNameId))
                    .ForMember(x => x.clientName, m => m.MapFrom(a => a.Name))
                    .ReverseMap()
                    .ForMember(x => x.ClientNameId, m => m.MapFrom(a => a.clientId))
                    .ForMember(x => x.Name, m => m.MapFrom(a => a.clientName));

                cfg.CreateMap<MoneyTransactionViewModel, TravelMoneyTransactions>()
                   .ForMember(x => x.Id, m => m.MapFrom(a => a.id))
                   .ForMember(x => x.CurrencyType, m => m.MapFrom(a => a.currencyType))
                   .ForMember(x => x.CurrencyGivenDays, m => m.MapFrom(a => a.currencyGivenDays))
                   .ForMember(x => x.ForexCardName, m => m.MapFrom(a => a.forexCardName))
                   .ForMember(x => x.CurrencygiveninCash, m => m.MapFrom(a => a.currencygiveninCash))
                   .ForMember(x => x.VendorName, m => m.MapFrom(a => a.vendorName))
                   .ForMember(x => x.TotalCurrencyCost, m => m.MapFrom(a => a.totalCurrencyCost))
                   .ForMember(x => x.Comments, m => m.MapFrom(a => a.comments))
                   .ReverseMap();

                cfg.CreateMap<UploadedDocumentViewModel, TravelUploads>()
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.description))
                    .ForMember(x => x.FileName, m => m.MapFrom(a => a.fileName))
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.id))
                    .ForMember(x => x.Url, m => m.MapFrom(a => a.url))
                    .ForMember(x => x.TravelId, m => m.MapFrom(a => a.travelId)).ReverseMap();

                cfg.CreateMap<TravelHotelBooking, HotelBooking>()
                    .ForMember(x => x.HotelId, m => m.MapFrom(a => a.hotelId))
                    .ForMember(x => x.Address, m => m.MapFrom(a => a.address))
                    .ForMember(x => x.CheckIn, m => m.MapFrom(a => a.checkIn))
                    .ForMember(x => x.CheckOut, m => m.MapFrom(a => a.checkOut))
                    .ForMember(x => x.EmailId, m => m.MapFrom(a => a.emailId))
                    .ForMember(x => x.HotelName, m => m.MapFrom(a => a.hotelName))
                    .ForMember(x => x.VendorName, m => m.MapFrom(a => a.vendorName))
                    .ForMember(x => x.Phone, m => m.MapFrom(a => a.phone))
                    .ForMember(x => x.OccupancyType, m => m.MapFrom(a => a.occupancyType))
                    .ForMember(x => x.Comment, m => m.MapFrom(a => a.comment))
                    .ForMember(x => x.TotalSummaryCost, m => m.MapFrom(a => a.totalSummaryCost))
                    .ReverseMap();

                cfg.CreateMap<TravelMediaPreferences, MediaPreferences>()
                    .ForMember(x => x.CableChannels, m => m.MapFrom(a => a.cableChannels))
                    .ForMember(x => x.Fax, m => m.MapFrom(a => a.fax))
                    .ForMember(x => x.FlatTV, m => m.MapFrom(a => a.flatTv))
                    .ForMember(x => x.Telephone, m => m.MapFrom(a => a.telephone))
                    .ForMember(x => x.MediaId, m => m.MapFrom(a => a.mediaId))
                    .ForMember(x => x.Media, m => m.MapFrom(a => a.media)).ReverseMap();

                cfg.CreateMap<TravelKitchenPreferences, KitchenPreferences>()
                    .ForMember(x => x.CoffeeMachine, m => m.MapFrom(a => a.coffeeMachine))
                    .ForMember(x => x.Microwave, m => m.MapFrom(a => a.microwave))
                    .ForMember(x => x.Refridgerator, m => m.MapFrom(a => a.refridgerator))
                    .ForMember(x => x.Utensils, m => m.MapFrom(a => a.utensils))
                    .ForMember(x => x.KitchenId, m => m.MapFrom(a => a.kitchenId))
                    .ForMember(x => x.Kitchen, m => m.MapFrom(a => a.kitchen)).ReverseMap();

                cfg.CreateMap<TravelServicesPreferences, ServicesPreferences>()
                    .ForMember(x => x.AtmOnSIte, m => m.MapFrom(a => a.atmOnSite))
                    .ForMember(x => x.Laundry, m => m.MapFrom(a => a.laundry))
                    .ForMember(x => x.PickUpDrop, m => m.MapFrom(a => a.pickUpDrop))
                    .ForMember(x => x.ServiceId, m => m.MapFrom(a => a.shoppingCenter))
                    .ForMember(x => x.ServiceId, m => m.MapFrom(a => a.serviceId))
                    .ForMember(x => x.Services, m => m.MapFrom(a => a.services)).ReverseMap();

                cfg.CreateMap<TravelInternetPreferences, InternetPreferences>()
                    .ForMember(x => x.Amount, m => m.MapFrom(a => a.amount))
                    .ForMember(x => x.FreeWiFi, m => m.MapFrom(a => a.freeWiFi))
                    .ForMember(x => x.InternetId, m => m.MapFrom(a => a.internetId))
                    .ForMember(x => x.PaidInternet, m => m.MapFrom(a => a.paidInternet))
                    .ForMember(x => x.Internet, m => m.MapFrom(a => a.internet)).ReverseMap();

                cfg.CreateMap<TravelGeneralPreferences, GeneralPreferences>()
                    .ForMember(x => x.GeneralId, m => m.MapFrom(a => a.generalId))
                    .ForMember(x => x.HairDryer, m => m.MapFrom(a => a.hairDryer))
                    .ForMember(x => x.Iron, m => m.MapFrom(a => a.iron))
                    .ForMember(x => x.IronFacilites, m => m.MapFrom(a => a.ironFacilites))
                    .ForMember(x => x.General, m => m.MapFrom(a => a.general)).ReverseMap();

                cfg.CreateMap<ApprisalQuestions, Pheonix.Models.ViewModels.AppraiseeQuestion>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Question, m => m.MapFrom(a => a.Questions));

                cfg.CreateMap<TravelFlight, Flight>()
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.id))                                     
                    .ForMember(x => x.TravelId, m => m.MapFrom(a => a.travelId))
                    .ForMember(x => x.TravelType, m => m.MapFrom(a => a.travelType))
                    .ForMember(x => x.TripType, m => m.MapFrom(a => a.tripType))
                    .ForMember(x => x.VendorName, m => m.MapFrom(a => a.vendorName))
                    .ForMember(x => x.Comments, m => m.MapFrom(a => a.comments))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.createdDate))
                    .ForMember(x => x.TotalFlightCost, m => m.MapFrom(a => a.totalFlightCost))
                    .ForMember(x => x.FlightDetails, m => m.MapFrom(a => a.flightDetails))
                    .ForMember(x => x.TravelInsurance, m => m.MapFrom(a => a.travelInsurance))
                    .ReverseMap();

                cfg.CreateMap<TravelFlightDetails, FlightDetails>()
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.id))
                    .ForMember(x => x.FlightId, m => m.MapFrom(a => a.flightId))
                    .ForMember(x => x.Source, m => m.MapFrom(a => a.source))
                    .ForMember(x => x.Destination, m => m.MapFrom(a => a.destination))
                    .ForMember(x => x.JourneyDate, m => m.MapFrom(a => a.journeyDate))
                    .ForMember(x => x.ReturnJourneyDate, m => m.MapFrom(a => a.returnJourneyDate))
                    .ReverseMap();

                cfg.CreateMap<TravelInsuranceclass, TravelInsurance>()
                   .ForMember(x => x.FlightId, m => m.MapFrom(a => a.flightId))
                   .ForMember(x => x.VendorName, m => m.MapFrom(a => a.vendorName))
                   .ForMember(x => x.StartDate, m => m.MapFrom(a => a.startDate))
                   .ForMember(x => x.EndDate, m => m.MapFrom(a => a.endDate))
                   .ForMember(x => x.TotalInsuranceCost, m => m.MapFrom(a => a.totalInsuranceCost))
                   .ReverseMap();

                cfg.CreateMap<Flight, TravelInsuranceclass>()
                   .ForMember(x => x.flightId, m => m.MapFrom(a => a.Id))
                   .ForMember(x => x.vendorName, m => m.MapFrom(a => a.VendorName))
                   .ReverseMap();
                 
                cfg.CreateMap<FlightClass, TravelFlightClass>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.title, m => m.MapFrom(a => a.Title)).ReverseMap();

                cfg.CreateMap<ApprovalDetail, ApprovalDetailViewModel>()
                    .ForMember(x => x.approvalDate, m => m.MapFrom(a => a.ApprovalDate))
                    .ForMember(x => x.approvalID, m => m.MapFrom(a => a.ApprovalID))
                    .ForMember(x => x.approverID, m => m.MapFrom(a => a.ApproverID))
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.isDeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.stage, m => m.MapFrom(a => a.Stage))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.statusComment, m => m.MapFrom(a => a.StatusComment)).ReverseMap();

                cfg.CreateMap<TravelExtensionHistory, TravelExtension>()
                    .ForMember(x => x.arrival, m => m.MapFrom(a => a.Arrival))
                    .ForMember(x => x.comments, m => m.MapFrom(a => a.Comments))
                    .ForMember(x => x.departure, m => m.MapFrom(a => a.Departure))
                    .ForMember(x => x.travelId, m => m.MapFrom(a => a.TravelDetailId))
                    .ForMember(x => x.id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.visaDate, m => m.MapFrom(a => a.Visa))
                    .ForMember(x => x.I94Date, m => m.MapFrom(a => a.I94))
                    .ReverseMap()
                    .ForMember(x => x.TravelDetailId, m => m.MapFrom(a => a.travelId))
                    .ForMember(x => x.Visa, m => m.MapFrom(a => a.visaDate))
                    .ForMember(x => x.I94, m => m.MapFrom(a => a.I94Date));

                cfg.CreateMap<AppraiseeAnswerModel, AppraiseForm>()
                   .ForMember(x => x.QuestionID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Answer, m => m.MapFrom(a => a.Answer));

                cfg.CreateMap<AppraisalReporting, AppraisalListModel>()
                  .ForMember(x => x.Appraiser, m => m.MapFrom(a => a.Person1.FirstName + " " + a.Person1.MiddleName + " " + a.Person1.LastName))
                  .ForMember(x => x.Reviewer, m => m.MapFrom(a => a.Person3.FirstName + " " + a.Person3.MiddleName + " " + a.Person3.LastName))
                  .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.AssignedTo, m => m.MapFrom(a => a.Person2.FirstName + " " + a.Person2.MiddleName + " " + a.Person2.LastName))
                 .ForMember(x => x.AppraiserScore, m => m.MapFrom(a => a.AppraiserRating))
                 .ForMember(x => x.ReviewerScore, m => m.MapFrom(a => a.ReviewerRating))
                 .ForMember(x => x.FinalReviewerRating, m => m.MapFrom(a => a.FinalReviewerRating));

                cfg.CreateMap<ApprisalParameters, Pheonix.Models.ViewModels.AppraiseeParametersViewModel>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.Parameter, m => m.MapFrom(a => a.Parameter))
                  .ForMember(x => x.Weightage, m => m.MapFrom(a => a.Weightage));

                cfg.CreateMap<AppraiseForm, AppraiseeFormModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Question, m => m.MapFrom(a => a.ApprisalQuestions.Questions))
                .ForMember(x => x.Answer, m => m.MapFrom(a => a.Answer));

                cfg.CreateMap<AppraiserForm, AppraiserFormModel>()
                .ForMember(x => x.AppraiseeId, m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<Pheonix.Models.ViewModels.AppraiseeParametersViewModel, AppraiserForm>()
                .ForMember(x => x.ParameterID, m => m.MapFrom(a => a.ID));

                cfg.CreateMap<SeatLocationPreferenceVM, SeatLocationPreference>()
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.description))
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.id)).ReverseMap();

                cfg.CreateMap<MealPreferenceVM, MealPreference>()
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.description))
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.id)).ReverseMap();

                cfg.CreateMap<AppraisalReporting, Pheonix.Models.ViewModels.AppraisalEmployeeViewModel>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                .ForMember(x => x.EmpID, m => m.MapFrom(a => a.PersonID))
                .ForMember(x => x.EmpName, m => m.MapFrom(a => a.EmpName))
                .ForMember(x => x.AppraiserId, m => m.MapFrom(a => a.AppraiserID))
                .ForMember(x => x.AppraiserName, m => m.MapFrom(a => a.AppraiserName))
                .ForMember(x => x.ReviewerId, m => m.MapFrom(a => a.ReviewerID))
                .ForMember(x => x.ReviewerName, m => m.MapFrom(a => a.ReviewerName))
                .ForMember(x => x.Grade, m => m.MapFrom(a => a.Grade))
                .ForMember(x => x.Location, m => m.MapFrom(a => a.Location))
                .ForMember(x => x.FreezedComment, m => m.MapFrom(a => a.FreezedComments))
                .ForMember(x => x.Designation, m => m.MapFrom(a => a.Designation))
                .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                .ReverseMap();

                cfg.CreateMap<GetAppraisalSummary_Result, PersonApraisal>()
                    .ForMember(x => x.fullName, m => m.MapFrom(a => a.EmpName))
                    .ForMember(x => x.personId, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.reviewerRating, m => m.MapFrom(a => a.ReviewerRating))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.image, m => m.MapFrom(a => a.Image));

                cfg.CreateMap<ApprovalDetailVM, ApprovalDetail>()
                    .ForMember(x => x.Stage, m => m.MapFrom(a => a.stage))
                    .ForMember(x => x.Status, m => m.MapFrom(a => a.status))
                    .ForMember(x => x.StatusComment, m => m.MapFrom(a => a.comment))
                    .ReverseMap()
                    .ForMember(x => x.comment, m => m.MapFrom(a => a.StatusComment));

                cfg.CreateMap<ProjectList, ProjectViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.ProjectName, m => m.MapFrom(a => a.ProjectName))
                    .ForMember(x => x.ProjectCode, m => m.MapFrom(a => a.ProjectCode))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.ProjectManager, m => m.MapFrom(a => a.ProjectManager))
                    .ForMember(x => x.ActualStartDate, m => m.MapFrom(a => a.ActualStartDate))
                    .ForMember(x => x.ActualEndDate, m => m.MapFrom(a => a.ActualEndDate))
                    .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                    .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.Billable, m => m.MapFrom(a => a.Billable))
                    .ForMember(x => x.DeliveryUnit, m => m.MapFrom(a => a.DeliveryUnit))
                    .ForMember(x => x.DeliveryTeam, m => m.MapFrom(a => a.DeliveryTeam))
                    .ForMember(x => x.IsExternal, m => m.MapFrom(a => a.IsExternal))
                    .ForMember(x => x.IsOffshore, m => m.MapFrom(a => a.IsOffshore))
                    .ForMember(x => x.ProjectType, m => m.MapFrom(a => a.ProjectType))
                    .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                    .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                    .ForMember(x => x.ParentProjId, m => m.MapFrom(a => a.ParentProjId))
                    .ForMember(x => x.ProjectMethodology, m => m.MapFrom(a => a.ProjectMethodology))
                    .ForMember(x => x.Process, m => m.MapFrom(a => a.Process))
                    .ForMember(x => x.SprintDuration, m => m.MapFrom(a => a.SprintDuration));

                cfg.CreateMap<ProjectViewModel, ProjectList>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.ProjectName, m => m.MapFrom(a => a.ProjectName))
                    .ForMember(x => x.ProjectCode, m => m.MapFrom(a => a.ProjectCode))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.ProjectManager, m => m.MapFrom(a => a.ProjectManager))
                    .ForMember(x => x.ActualStartDate, m => m.MapFrom(a => a.ActualStartDate))
                    .ForMember(x => x.ActualEndDate, m => m.MapFrom(a => a.ActualEndDate))
                    .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                    .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.Billable, m => m.MapFrom(a => a.Billable))
                    .ForMember(x => x.DeliveryUnit, m => m.MapFrom(a => a.DeliveryUnit))
                    .ForMember(x => x.DeliveryTeam, m => m.MapFrom(a => a.DeliveryTeam))
                    .ForMember(x => x.IsExternal, m => m.MapFrom(a => a.IsExternal))
                    .ForMember(x => x.IsOffshore, m => m.MapFrom(a => a.IsOffshore))
                    .ForMember(x => x.ProjectType, m => m.MapFrom(a => a.ProjectType))
                    .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                    .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                    .ForMember(x => x.ParentProjId, m => m.MapFrom(a => a.ParentProjId))
                    .ForMember(x => x.ProjectMethodology, m => m.MapFrom(a => a.ProjectMethodology))
                    .ForMember(x => x.Process, m => m.MapFrom(a => a.Process))
                    .ForMember(x => x.SprintDuration, m => m.MapFrom(a => a.SprintDuration));

                cfg.CreateMap<PMSTasks, Pheonix.Models.VM.Classes.Task.TasksViewModel>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.srno, m => m.MapFrom(a => a.SrNo))
                    .ForMember(x => x.taskname, m => m.MapFrom(a => a.TaskName))
                    .ForMember(x => x.featuretype, m => m.MapFrom(a => a.FeatureType))
                    .ForMember(x => x.description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.defaultFields, m => m.MapFrom(a => a.DefaultFields))
                    .ForMember(x => x.acceptance, m => m.MapFrom(a => a.Acceptance))
                    .ForMember(x => x.createdDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.isdeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.hours, m => m.MapFrom(a => a.Hours))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.projectId, m => m.MapFrom(a => a.ProjectId))
                    .ReverseMap();

                cfg.CreateMap<PMSTasks, Pheonix.Models.VM.Classes.Task.TasksListModel>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.srno, m => m.MapFrom(a => a.SrNo))
                    .ForMember(x => x.taskname, m => m.MapFrom(a => a.TaskName))
                    .ForMember(x => x.featuretype, m => m.MapFrom(a => a.FeatureType))
                    .ForMember(x => x.description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.defaultFields, m => m.MapFrom(a => a.DefaultFields))
                    .ForMember(x => x.acceptance, m => m.MapFrom(a => a.Acceptance))
                    .ForMember(x => x.createdDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.isdeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.hours, m => m.MapFrom(a => a.Hours))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.projectId, m => m.MapFrom(a => a.ProjectId))
                    .ReverseMap();

                cfg.CreateMap<PMSTimesheet, Pheonix.Models.VM.Classes.Timesheet.TimesheetViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.NonBillableDescription, m => m.MapFrom(a => a.NonBillableDescription))
                    .ForMember(x => x.TotalHours, m => m.MapFrom(a => a.Time))
                    .ForMember(x => x.NonBillableTime, m => m.MapFrom(a => a.NonBillableTime))
                    .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                    .ForMember(x => x.IsEmailSent, m => m.MapFrom(a => a.IsEmailSent))
                    .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                    .ForMember(x => x.TaskTypeId, m => m.MapFrom(a => a.TaskTypeId))
                    .ForMember(x => x.SubTaskId, m => m.MapFrom(a => a.SubTaskId))
                    .ForMember(x => x.JsonString, m => m.MapFrom(a => a.JsonString))
                    .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.SubProjectID, m => m.MapFrom(a => a.SubProjectID))
                    .ForMember(x => x.TicketID, m => m.MapFrom(a => a.TicketID))
                    .ReverseMap();

                cfg.CreateMap<Pheonix.Models.VM.Classes.Timesheet.TimesheetViewModel, PMSTimesheet>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.NonBillableDescription, m => m.MapFrom(a => a.NonBillableDescription))
                    .ForMember(x => x.Time, m => m.MapFrom(a => a.TotalHours))
                    .ForMember(x => x.NonBillableTime, m => m.MapFrom(a => a.NonBillableTime))
                    .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                    .ForMember(x => x.IsEmailSent, m => m.MapFrom(a => a.IsEmailSent))
                    .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                    .ForMember(x => x.TaskTypeId, m => m.MapFrom(a => a.TaskTypeId))
                    .ForMember(x => x.SubTaskId, m => m.MapFrom(a => a.SubTaskId))
                    .ForMember(x => x.JsonString, m => m.MapFrom(a => a.JsonString))
                    .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.SubProjectID, m => m.MapFrom(a => a.SubProjectID))
                    .ForMember(x => x.TicketID, m => m.MapFrom(a => a.TicketID));

                cfg.CreateMap<Pheonix.Models.VM.Classes.Timesheet.TimesheetMultipleEntryViewModel, PMSTimesheet>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.NonBillableDescription, m => m.MapFrom(a => a.NonBillableDescription))
                    .ForMember(x => x.Time, m => m.MapFrom(a => a.TotalHours))
                    .ForMember(x => x.NonBillableTime, m => m.MapFrom(a => a.NonBillableTime))
                    .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                    .ForMember(x => x.IsEmailSent, m => m.MapFrom(a => a.IsEmailSent))
                    .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                    .ForMember(x => x.TaskTypeId, m => m.MapFrom(a => a.TaskTypeId))
                    .ForMember(x => x.SubTaskId, m => m.MapFrom(a => a.SubTaskId))
                    .ForMember(x => x.JsonString, m => m.MapFrom(a => a.JsonString))
                    .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.SubProjectID, m => m.MapFrom(a => a.SubProjectID))
                    .ForMember(x => x.TicketID, m => m.MapFrom(a => a.TicketID))
                    .ForMember(x => x.UploadType, m => m.MapFrom(a => a.UploadType));

                cfg.CreateMap<CustomerViewModel, Customer>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.CustomerCode, m => m.MapFrom(a => a.CustomerCode))
                   .ForMember(x => x.ExternalMarketSegmentation, m => m.MapFrom(a => a.ExternalMarketSegmentation))
                   .ForMember(x => x.CustomerRegion, m => m.MapFrom(a => a.CustomerRegion))
                   .ForMember(x => x.ContractEffectiveDate, m => m.MapFrom(a => a.ContractEffectiveDate))
                   .ForMember(x => x.ValidTill, m => m.MapFrom(a => a.ValidTill))
                   .ForMember(x => x.CreditPeriod, m => m.MapFrom(a => a.CreditPeriod))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                   .ForMember(x => x.Status, m => m.MapFrom(a => a.Status));

                cfg.CreateMap<Customer, CustomerViewModel>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.CustomerCode, m => m.MapFrom(a => a.CustomerCode))
                   .ForMember(x => x.ExternalMarketSegmentation, m => m.MapFrom(a => a.ExternalMarketSegmentation))
                   .ForMember(x => x.CustomerRegion, m => m.MapFrom(a => a.CustomerRegion))
                   .ForMember(x => x.ContractEffectiveDate, m => m.MapFrom(a => a.ContractEffectiveDate))
                   .ForMember(x => x.ValidTill, m => m.MapFrom(a => a.ValidTill))
                   .ForMember(x => x.CreditPeriod, m => m.MapFrom(a => a.CreditPeriod))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                   .ForMember(x => x.Status, m => m.MapFrom(a => a.Status));

                cfg.CreateMap<CustomerAddress, CustomerAddressViewModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                  .ForMember(x => x.Address, m => m.MapFrom(a => a.Address))
                  .ForMember(x => x.City, m => m.MapFrom(a => a.City))
                  .ForMember(x => x.State, m => m.MapFrom(a => a.State))
                  .ForMember(x => x.ZipCode, m => m.MapFrom(a => a.ZipCode))
                  .ForMember(x => x.CountryID, m => m.MapFrom(a => a.CountryID));

                cfg.CreateMap<CustomerAddressViewModel, CustomerAddress>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                  .ForMember(x => x.Address, m => m.MapFrom(a => a.Address))
                  .ForMember(x => x.City, m => m.MapFrom(a => a.City))
                  .ForMember(x => x.State, m => m.MapFrom(a => a.State))
                  .ForMember(x => x.ZipCode, m => m.MapFrom(a => a.ZipCode))
                  .ForMember(x => x.CountryID, m => m.MapFrom(a => a.CountryID));

                cfg.CreateMap<CustomerContactPerson, CustomerContactPersonViewModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                  .ForMember(x => x.FirstName, m => m.MapFrom(a => a.FirstName))
                  .ForMember(x => x.LastName, m => m.MapFrom(a => a.LastName))
                  .ForMember(x => x.Type, m => m.MapFrom(a => a.Type))
                  .ForMember(x => x.Designation, m => m.MapFrom(a => a.Designation))
                  .ForMember(x => x.Role, m => m.MapFrom(a => a.Role))
                  .ForMember(x => x.Skype, m => m.MapFrom(a => a.Skype))
                  .ForMember(x => x.Email, m => m.MapFrom(a => a.Email))
                  .ForMember(x => x.OfficeTelephone, m => m.MapFrom(a => a.OfficeTelephone))
                  .ForMember(x => x.Telephone, m => m.MapFrom(a => a.Telephone))
                  .ForMember(x => x.Mobile, m => m.MapFrom(a => a.Mobile))
                  .ForMember(x => x.FaxNo, m => m.MapFrom(a => a.FaxNo))
                  .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted));

                cfg.CreateMap<CustomerContactPersonViewModel, CustomerContactPerson>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                  .ForMember(x => x.FirstName, m => m.MapFrom(a => a.FirstName))
                   .ForMember(x => x.LastName, m => m.MapFrom(a => a.LastName))
                  .ForMember(x => x.Type, m => m.MapFrom(a => a.Type))
                  .ForMember(x => x.Designation, m => m.MapFrom(a => a.Designation))
                  .ForMember(x => x.Role, m => m.MapFrom(a => a.Role))
                  .ForMember(x => x.Skype, m => m.MapFrom(a => a.Skype))
                  .ForMember(x => x.Email, m => m.MapFrom(a => a.Email))
                  .ForMember(x => x.OfficeTelephone, m => m.MapFrom(a => a.OfficeTelephone))
                  .ForMember(x => x.Telephone, m => m.MapFrom(a => a.Telephone))
                  .ForMember(x => x.Mobile, m => m.MapFrom(a => a.Mobile))
                   .ForMember(x => x.FaxNo, m => m.MapFrom(a => a.FaxNo))
                   .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted));

                cfg.CreateMap<CustomerContract, CustomerContractViewModel>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                 .ForMember(x => x.ContractName, m => m.MapFrom(a => a.ContractName))
                 .ForMember(x => x.ContractType, m => m.MapFrom(a => a.ContractType))
                 .ForMember(x => x.ContractSummary, m => m.MapFrom(a => a.ContractSummary))
                 .ForMember(x => x.ContractDetails, m => m.MapFrom(a => a.ContractDetails))
                 .ForMember(x => x.CommencementDate, m => m.MapFrom(a => a.CommencementDate))
                 .ForMember(x => x.DateEffective, m => m.MapFrom(a => a.DateEffective))
                 .ForMember(x => x.DateValidTill, m => m.MapFrom(a => a.DateValidTill))
                 .ForMember(x => x.ContractFile, m => m.MapFrom(a => a.ContractFile))
                 .ForMember(x => x.ContractFiles, m => m.MapFrom(a => a.ContractAttachments));

                cfg.CreateMap<CustomerContractViewModel, CustomerContract>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.CustomerID, m => m.MapFrom(a => a.CustomerID))
                 .ForMember(x => x.ContractName, m => m.MapFrom(a => a.ContractName))
                 .ForMember(x => x.ContractType, m => m.MapFrom(a => a.ContractType))
                 .ForMember(x => x.ContractSummary, m => m.MapFrom(a => a.ContractSummary))
                 .ForMember(x => x.ContractDetails, m => m.MapFrom(a => a.ContractDetails))
                 .ForMember(x => x.CommencementDate, m => m.MapFrom(a => a.CommencementDate))
                 .ForMember(x => x.DateEffective, m => m.MapFrom(a => a.DateEffective))
                 .ForMember(x => x.DateValidTill, m => m.MapFrom(a => a.DateValidTill))
                 .ForMember(x => x.ContractFile, m => m.MapFrom(a => a.ContractFile))
                 .ForMember(x => x.ContractAttachments, m => m.MapFrom(a => a.ContractFiles));

                cfg.CreateMap<CompOff, CompOffViewModel>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                 .ForMember(x => x.ExpiresOn, m => m.MapFrom(a => a.ExpiresOn))
                 .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                 .ForMember(x => x.ForDate, m => m.MapFrom(a => a.ForDate))
                 .ForMember(x => x.IsApplied, m => m.MapFrom(a => a.IsApplied))
                 .ForMember(x => x.Narration, m => m.MapFrom(a => a.Narration));

                cfg.CreateMap<DeletedRecordsLogDetails, DeletedRecordsLogDetailViewModel>()
                           .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                              .ForMember(x => x.ModuleID, m => m.MapFrom(a => a.ModuleID))
                              .ForMember(x => x.DeletedRecordID, m => m.MapFrom(a => a.DeletedRecordID))
                              .ForMember(x => x.DeletedOn, m => m.MapFrom(a => a.DeletedOn))
                              .ForMember(x => x.DeletedBy, m => m.MapFrom(a => a.DeletedBy));

                cfg.CreateMap<DeletedRecordsLogDetailViewModel, DeletedRecordsLogDetails>()
                       .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                          .ForMember(x => x.ModuleID, m => m.MapFrom(a => a.ModuleID))
                          .ForMember(x => x.DeletedRecordID, m => m.MapFrom(a => a.DeletedRecordID))
                          .ForMember(x => x.DeletedOn, m => m.MapFrom(a => a.DeletedOn))
                          .ForMember(x => x.DeletedBy, m => m.MapFrom(a => a.DeletedBy));

                cfg.CreateMap<PMSTasks, Pheonix.Models.VM.Classes.Task.TasksViewModel>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.srno, m => m.MapFrom(a => a.SrNo))
                    .ForMember(x => x.taskname, m => m.MapFrom(a => a.TaskName))
                    .ForMember(x => x.featuretype, m => m.MapFrom(a => a.FeatureType))
                    .ForMember(x => x.description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.defaultFields, m => m.MapFrom(a => a.DefaultFields))
                    .ForMember(x => x.acceptance, m => m.MapFrom(a => a.Acceptance))
                    .ForMember(x => x.createdDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.isdeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.hours, m => m.MapFrom(a => a.Hours))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.projectId, m => m.MapFrom(a => a.ProjectId))
                    .ReverseMap();

                cfg.CreateMap<PMSTasks, Pheonix.Models.VM.Classes.Task.TasksListModel>()
                    .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.srno, m => m.MapFrom(a => a.SrNo))
                    .ForMember(x => x.taskname, m => m.MapFrom(a => a.TaskName))
                    .ForMember(x => x.featuretype, m => m.MapFrom(a => a.FeatureType))
                    .ForMember(x => x.description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.defaultFields, m => m.MapFrom(a => a.DefaultFields))
                    .ForMember(x => x.acceptance, m => m.MapFrom(a => a.Acceptance))
                    .ForMember(x => x.createdDate, m => m.MapFrom(a => a.CreatedDate))
                    .ForMember(x => x.isdeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.hours, m => m.MapFrom(a => a.Hours))
                    .ForMember(x => x.status, m => m.MapFrom(a => a.Status))
                    .ForMember(x => x.projectId, m => m.MapFrom(a => a.ProjectId))
                    .ReverseMap();

                cfg.CreateMap<PMSAllocationRequest, Pheonix.Models.VM.Classes.ResourceAllocation.ResourceAllocationModel>()
                   .ForMember(x => x.RequestID, m => m.MapFrom(a => a.RequestID))
                   .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                   .ForMember(x => x.RequestedBy, m => m.MapFrom(a => a.RequestedBy))
                  .ForMember(x => x.RequestDate, m => m.MapFrom(a => a.RequestDate))
                   .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                   .ForMember(x => x.RequestType, m => m.MapFrom(a => a.RequestType))
                    .ForMember(x => x.StatusDate, m => m.MapFrom(a => a.StatusDate))
                  .ReverseMap();

                cfg.CreateMap<PMSAllocationRequestDetails, Pheonix.Models.VM.Classes.ResourceAllocation.ResourceAllocationModel>()
                   .ForMember(x => x.RequestID, m => m.MapFrom(a => a.RequestID))
                   .ForMember(x => x.id, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.empid, m => m.MapFrom(a => a.EmpID))
                  .ForMember(x => x.percentage, m => m.MapFrom(a => a.Percentage))
                   .ForMember(x => x.fromdate, m => m.MapFrom(a => a.FromDate))
                   .ForMember(x => x.ToDate, m => m.MapFrom(a => a.ToDate))
                    .ForMember(x => x.BillableType, m => m.MapFrom(a => a.BillableType))
                    .ForMember(x => x.ProjectRole, m => m.MapFrom(a => a.ProjectRole))
                   .ForMember(x => x.ReportingTo, m => m.MapFrom(a => a.ReportingTo))
                   .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.RequestID, m => m.MapFrom(a => a.RequestID))
                  .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                  .ForMember(x => x.RMGCOmments, m => m.MapFrom(a => a.RMGCOmments))
                  .ForMember(x => x.StatusDate, m => m.MapFrom(a => a.StatusDate))
                  .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                  .ReverseMap();

                cfg.CreateMap<GetTimesheet_Result, Pheonix.Models.VM.Classes.Timesheet.TimesheetViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                   .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.NonBillableDescription, m => m.MapFrom(a => a.NonBillableDescription))
                   .ForMember(x => x.TotalHours, m => m.MapFrom(a => a.TotalHours)).ReverseMap()
                   .ForMember(x => x.NonBillableTime, m => m.MapFrom(a => a.NonBillableTime)).ReverseMap()
                   .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                   .ForMember(x => x.SubProjectID, m => m.MapFrom(a => a.SubProjectID))
                   .ForMember(x => x.IsEmailSent, m => m.MapFrom(a => a.IsEmailSent))
                   .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                   .ForMember(x => x.TaskTypeId, m => m.MapFrom(a => a.TaskTypeId))
                   .ForMember(x => x.SubTaskId, m => m.MapFrom(a => a.SubTaskId))
                   .ForMember(x => x.JsonString, m => m.MapFrom(a => a.JsonString))
                   .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                   .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                   .ForMember(x => x.EmployeeName, m => m.MapFrom(a => a.EmployeeName))
                   .ForMember(x => x.TicketID, m => m.MapFrom(a => a.TicketID))
                  .ReverseMap();

                cfg.CreateMap<GetTimesheetByProjectID_Result, Pheonix.Models.VM.Classes.Timesheet.TimesheetViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                   .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.NonBillableDescription, m => m.MapFrom(a => a.NonBillableDescription))
                   .ForMember(x => x.TotalHours, m => m.MapFrom(a => a.TotalHours)).ReverseMap()
                   .ForMember(x => x.NonBillableTime, m => m.MapFrom(a => a.NonBillableTime)).ReverseMap()
                   .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                   .ForMember(x => x.SubProjectID, m => m.MapFrom(a => a.SubProjectID))
                   .ForMember(x => x.IsEmailSent, m => m.MapFrom(a => a.IsEmailSent))
                   .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                   .ForMember(x => x.TaskTypeId, m => m.MapFrom(a => a.TaskTypeId))
                   .ForMember(x => x.SubTaskId, m => m.MapFrom(a => a.SubTaskId))
                   .ForMember(x => x.JsonString, m => m.MapFrom(a => a.JsonString))
                   .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                   .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                   .ForMember(x => x.EmployeeName, m => m.MapFrom(a => a.EmployeeName))
                   .ForMember(x => x.ProjectName, m => m.MapFrom(a => a.ProjectName))
                   .ForMember(x => x.SubProjectName, m => m.MapFrom(a => a.SubProjectName))
                   .ForMember(x => x.TaskType, m => m.MapFrom(a => a.TaskType))
                   .ForMember(x => x.SubTaskType, m => m.MapFrom(a => a.SubTaskType))
                   .ForMember(x => x.HourSum, m => m.MapFrom(a => a.HourSum)).ReverseMap()
                  .ReverseMap();

                cfg.CreateMap<SeperationConfig, SeperationConfigViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.ChecklistItem, m => m.MapFrom(a => a.ChecklistItem))
                    .ForMember(x => x.RoleID, m => m.MapFrom(a => a.RoleID))
                    .ForMember(x => x.IsActive, m => m.MapFrom(a => a.IsActive))
                    .ReverseMap();

                cfg.CreateMap<SeperationProcess, SeperationConfigProcessViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.RoleID, m => m.MapFrom(a => a.RoleID))
                    .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                    .ReverseMap();

                cfg.CreateMap<Role, RoleViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.SliceFromRole, m => m.MapFrom(a => a.SliceFromRole))
                    .ForMember(x => x.From, m => m.MapFrom(a => a.From))
                    .ForMember(x => x.To, m => m.MapFrom(a => a.To))
                    .ForMember(x => x.IsTemporary, m => m.MapFrom(a => a.IsTemporary))
                    .ReverseMap();

                cfg.CreateMap<Separation, Pheonix.Models.ViewModels.SeperationViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.ResignDate, m => m.MapFrom(a => a.ResignDate))
                    .ForMember(x => x.SeperationReason, m => m.MapFrom(a => a.SeperationReason))
                    .ForMember(x => x.StatusID, m => m.MapFrom(a => a.StatusID))
                    .ForMember(x => x.UpdatedBy, m => m.MapFrom(a => a.UpdatedBy))
                    .ForMember(x => x.UpdatedOn, m => m.MapFrom(a => a.UpdatedOn))
                    .ForMember(x => x.ActualDate, m => m.MapFrom(a => a.ActualDate))
                    .ForMember(x => x.ApprovalDate, m => m.MapFrom(a => a.ApprovalDate))
                    .ForMember(x => x.ApprovalID, m => m.MapFrom(a => a.ApprovalID))
                    .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                    .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                    .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                    .ForMember(x => x.ExpectedDate, m => m.MapFrom(a => a.ExpectedDate))
                    .ForMember(x => x.RejectRemark, m => m.MapFrom(a => a.RejectRemark))
                    .ForMember(x => x.WithdrawRemark, m => m.MapFrom(a => a.WithdrawRemark))
                    .ForMember(x => x.IsTermination, m => m.MapFrom(a => a.IsTermination))
                    .ForMember(x => x.TerminationRemark, m => m.MapFrom(a => a.TerminationRemark))
                    .ForMember(x => x.TerminationReason, m => m.MapFrom(a => a.TerminationReason))
                    .ForMember(x => x.NoticePeriod, m => m.MapFrom(a => a.NoticePeriod))
                    .ForMember(x => x.OldEmploymentStatus, m => m.MapFrom(a => a.OldEmploymentStatus))
                    .ReverseMap();

                cfg.CreateMap<SeperationProcess, Pheonix.Models.ViewModels.SeperationProcessViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.RoleID, m => m.MapFrom(a => a.RoleID))
                    .ForMember(x => x.SeperationID, m => m.MapFrom(a => a.SeperationID))
                    .ForMember(x => x.StatusID, m => m.MapFrom(a => a.StatusID))
                    .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                    .ForMember(x => x.ChecklistProcessedData, m => m.MapFrom(a => a.ChecklistProcessedData))
                    .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                    .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                    .ForMember(x => x.UpdatedBy, m => m.MapFrom(a => a.UpdatedBy))
                    .ForMember(x => x.UpdatedOn, m => m.MapFrom(a => a.UpdatedOn))
                    .ReverseMap();

                cfg.CreateMap<HolidayList, HolidayListModel>()
                 .ForMember(x => x.Date, m => m.MapFrom(a => a.Date))
                 .ForMember(x => x.Description, m => m.MapFrom(a => a.Description));

                cfg.CreateMap<PhoenixConfig, AdminLeaveConfigModel>()
                .ForMember(x => x.ConfigKey, m => m.MapFrom(a => a.ConfigKey))
                .ForMember(x => x.ConfigValue, m => m.MapFrom(a => a.ConfigValue))
                .ForMember(x => x.Location, m => m.MapFrom(a => a.Location))
                .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
                .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                .ForMember(x => x.Year, m => m.MapFrom(a => a.Year));

                cfg.CreateMap<AdminLeaveConfigModel, PhoenixConfig>()
               .ForMember(x => x.ConfigKey, m => m.MapFrom(a => a.ConfigKey))
               .ForMember(x => x.ConfigValue, m => m.MapFrom(a => a.ConfigValue))
               .ForMember(x => x.Location, m => m.MapFrom(a => a.Location))
               .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
               .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
               .ForMember(x => x.Year, m => m.MapFrom(a => a.Year));

                cfg.CreateMap<CompOffExceptionViewModel, CompensatoryOffException>()
                .ForMember(x => x.PersonID, m => m.MapFrom(a => a.ID));

                cfg.CreateMap<CompensatoryOffException, CompOffExceptionViewModel>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.PersonID));

                cfg.CreateMap<ReportSeparationDataModel, Pheonix.Models.ViewModels.SeperationViewModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.PersonID,
                              m => m.MapFrom(a => a.PersonID))
                   .ForMember(x => x.ActualDate,
                              m => m.MapFrom(a => a.ActualDate))
                   .ReverseMap();

                cfg.CreateMap<ReportSeparationDataModel, EmployeeBasicProfile>()
                   .ForMember(x => x.FirstName,
                              m => m.MapFrom(a => a.FirstName))
                   .ForMember(x => x.LastName,
                              m => m.MapFrom(a => a.LastName))
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.PersonID))
                   .ForMember(x => x.joiningDate,
                              m => m.MapFrom(a => a.JoiningDate))
                   .ForMember(x => x.CurrentDesignation,
                              m => m.MapFrom(a => a.CurrentDesignation))
                   .ReverseMap();

                cfg.CreateMap<PMSConfiguration, PMSConfigurationViewModel>()
                   .ForMember(x => x.Id,
                              m => m.MapFrom(a => a.Id))
                   .ForMember(x => x.Project,
                              m => m.MapFrom(a => a.Project))
                   .ForMember(x => x.Role,
                              m => m.MapFrom(a => a.Role))
                   .ForMember(x => x.PersonID,
                              m => m.MapFrom(a => a.PersonID))
                   .ReverseMap();

                cfg.CreateMap<PMSConfigurationViewModel, PMSConfiguration>()
                   .ForMember(x => x.Id,
                              m => m.MapFrom(a => a.Id))
                   .ForMember(x => x.Project,
                              m => m.MapFrom(a => a.Project))
                   .ForMember(x => x.Role,
                              m => m.MapFrom(a => a.Role))
                   .ForMember(x => x.PersonID,
                              m => m.MapFrom(a => a.PersonID))
                   .ReverseMap();

                cfg.CreateMap<SeparationReasons, SeparationReasonViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.ReasonDescription, m => m.MapFrom(a => a.ReasonDescription))
                   .ForMember(x => x.ReasonCode, m => m.MapFrom(a => a.ReasonCode))
                   .ForMember(x => x.IsActive, m => m.MapFrom(a => a.IsActive))
                   .ForMember(x => x.ReasonCodeID, m => m.MapFrom(a => a.ReasonCodeID))
                   .ReverseMap();

                cfg.CreateMap<ReportSeparationDataModel, EmployeeBasicProfile>()
                  .ForMember(x => x.ID,
                             m => m.MapFrom(a => a.PersonID))
                  .ForMember(x => x.joiningDate,
                             m => m.MapFrom(a => a.JoiningDate))
                  .ForMember(x => x.CurrentDesignation,
                             m => m.MapFrom(a => a.CurrentDesignation))
                  .ReverseMap();

                cfg.CreateMap<FeedbackForLeavingOrg, FeedbackForLeavingOrgViewModel>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                  .ForMember(x => x.FutherStudies, m => m.MapFrom(a => a.FutherStudies))
                  .ForMember(x => x.FutherStudiesComment, m => m.MapFrom(a => a.FutherStudiesComment))
                  .ForMember(x => x.HealthFactor, m => m.MapFrom(a => a.HealthFactor))
                  .ForMember(x => x.HealthFactorComment, m => m.MapFrom(a => a.HealthFactorComment))
                  .ForMember(x => x.MarriageFamily, m => m.MapFrom(a => a.MarriageFamily))
                  .ForMember(x => x.MarriageFamilyComment, m => m.MapFrom(a => a.MarriageFamilyComment))
                  .ForMember(x => x.OpportunitySalary, m => m.MapFrom(a => a.OpportunitySalary))
                  .ForMember(x => x.OpportunitySalaryComment, m => m.MapFrom(a => a.OpportunitySalaryComment))
                  .ForMember(x => x.Overseas, m => m.MapFrom(a => a.Overseas))
                  .ForMember(x => x.OverseasComment, m => m.MapFrom(a => a.OverseasComment))
                  .ForMember(x => x.JobExpectation, m => m.MapFrom(a => a.JobExpectation))
                  .ForMember(x => x.JobExpectationComment, m => m.MapFrom(a => a.JobExpectationComment))
                  .ForMember(x => x.Work, m => m.MapFrom(a => a.Work))
                  .ForMember(x => x.WorkComment, m => m.MapFrom(a => a.WorkComment))
                  .ForMember(x => x.WorkCulture, m => m.MapFrom(a => a.WorkCulture))
                  .ForMember(x => x.WorkCultureComment, m => m.MapFrom(a => a.WorkCultureComment))
                  .ForMember(x => x.WorkHours, m => m.MapFrom(a => a.WorkHours))
                  .ForMember(x => x.WorkHoursComment, m => m.MapFrom(a => a.WorkHoursComment))
                  .ForMember(x => x.Responsibilities, m => m.MapFrom(a => a.Responsibilities))
                  .ForMember(x => x.ResponsibilitiesComment, m => m.MapFrom(a => a.ResponsibilitiesComment))
                  .ForMember(x => x.TeamIssues, m => m.MapFrom(a => a.TeamIssues))
                  .ForMember(x => x.TeamIssuesComment, m => m.MapFrom(a => a.TeamIssuesComment))
                  .ForMember(x => x.Recognition, m => m.MapFrom(a => a.Recognition))
                  .ForMember(x => x.RecognitionComment, m => m.MapFrom(a => a.RecognitionComment))
                  .ForMember(x => x.ReportingHeadIssues, m => m.MapFrom(a => a.ReportingHeadIssues))
                  .ForMember(x => x.ReportingHeadIssuesComment, m => m.MapFrom(a => a.ReportingHeadIssuesComment))
                  .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                  .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                  .ReverseMap();

                cfg.CreateMap<RatingForReportingLead, RatingForReportingLeadViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                   .ForMember(x => x.CommunicationExpectation, m => m.MapFrom(a => a.CommunicationExpectation))
                   .ForMember(x => x.ResolvedComplaints, m => m.MapFrom(a => a.ResolvedComplaints))
                   .ForMember(x => x.ResolvedComplaintsComment, m => m.MapFrom(a => a.ResolvedComplaintsComment))
                   .ForMember(x => x.Teamwork, m => m.MapFrom(a => a.Teamwork))
                   .ForMember(x => x.TeamworkComment, m => m.MapFrom(a => a.TeamworkComment))
                   .ForMember(x => x.Approachable, m => m.MapFrom(a => a.Approachable))
                   .ForMember(x => x.ApproachableComment, m => m.MapFrom(a => a.ApproachableComment))
                   .ForMember(x => x.Receptive, m => m.MapFrom(a => a.Receptive))
                   .ForMember(x => x.ReceptiveComment, m => m.MapFrom(a => a.ReceptiveComment))
                   .ForMember(x => x.TechnicalGuidance, m => m.MapFrom(a => a.TechnicalGuidance))
                   .ForMember(x => x.TechnicalGuidanceComment, m => m.MapFrom(a => a.TechnicalGuidanceComment))
                   .ForMember(x => x.DevelopedYou, m => m.MapFrom(a => a.DevelopedYou))
                   .ForMember(x => x.DevelopedYouComment, m => m.MapFrom(a => a.DevelopedYouComment))
                   .ForMember(x => x.PerformanceFeedback, m => m.MapFrom(a => a.PerformanceFeedback))
                   .ForMember(x => x.PerformanceFeedbackComment, m => m.MapFrom(a => a.PerformanceFeedbackComment))
                   .ForMember(x => x.Accomplishments, m => m.MapFrom(a => a.Accomplishments))
                   .ForMember(x => x.AccomplishmentsComment, m => m.MapFrom(a => a.AccomplishmentsComment))
                   .ForMember(x => x.Assignments, m => m.MapFrom(a => a.Assignments))
                   .ForMember(x => x.AssignmentsComment, m => m.MapFrom(a => a.AssignmentsComment))
                   .ForMember(x => x.EffectiveLeader, m => m.MapFrom(a => a.EffectiveLeader))
                   .ForMember(x => x.EffectiveLeaderComment, m => m.MapFrom(a => a.EffectiveLeaderComment))
                   .ForMember(x => x.SensitiveToEmployee, m => m.MapFrom(a => a.SensitiveToEmployee))
                   .ForMember(x => x.SensitiveToEmployeeComment, m => m.MapFrom(a => a.SensitiveToEmployeeComment))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ReverseMap();

                cfg.CreateMap<RatingForOrganization, RatingForOrganizationViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                   .ForMember(x => x.BenefitsAndPay, m => m.MapFrom(a => a.BenefitsAndPay))
                   .ForMember(x => x.BenefitsAndPayComment, m => m.MapFrom(a => a.BenefitsAndPayComment))
                   .ForMember(x => x.TrainingSessions, m => m.MapFrom(a => a.TrainingSessions))
                   .ForMember(x => x.TrainingSessionsComment, m => m.MapFrom(a => a.TrainingSessionsComment))
                   .ForMember(x => x.OrgReviewSystem, m => m.MapFrom(a => a.OrgReviewSystem))
                   .ForMember(x => x.OrgReviewSystemComment, m => m.MapFrom(a => a.OrgReviewSystemComment))
                   .ForMember(x => x.OpenDoorPolicy, m => m.MapFrom(a => a.OpenDoorPolicy))
                   .ForMember(x => x.OpenDoorPolicyComment, m => m.MapFrom(a => a.OpenDoorPolicyComment))
                   .ForMember(x => x.JobPayment, m => m.MapFrom(a => a.JobPayment))
                   .ForMember(x => x.JobPaymentComment, m => m.MapFrom(a => a.JobPaymentComment))
                   .ForMember(x => x.CareerOpportunity, m => m.MapFrom(a => a.CareerOpportunity))
                   .ForMember(x => x.CareerOpportunityComment, m => m.MapFrom(a => a.CareerOpportunityComment))
                   .ForMember(x => x.WorkingConditions, m => m.MapFrom(a => a.WorkingConditions))
                   .ForMember(x => x.WorkingConditionsComment, m => m.MapFrom(a => a.WorkingConditionsComment))
                   .ForMember(x => x.OrganizationGrowth, m => m.MapFrom(a => a.OrganizationGrowth))
                   .ForMember(x => x.OrganizationGrowthComment, m => m.MapFrom(a => a.OrganizationGrowthComment))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ReverseMap();

                cfg.CreateMap<OrganizationDevelopmentSuggestion, OrgDevelopmentSuggestionViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                   .ForMember(x => x.Question1, m => m.MapFrom(a => a.Question1))
                   .ForMember(x => x.Question1Comment, m => m.MapFrom(a => a.Question1Comment))
                   .ForMember(x => x.Question2, m => m.MapFrom(a => a.Question2))
                   .ForMember(x => x.Question2Comment, m => m.MapFrom(a => a.Question2Comment))
                   .ForMember(x => x.Question3, m => m.MapFrom(a => a.Question3))
                   .ForMember(x => x.Question3Comment, m => m.MapFrom(a => a.Question3Comment))
                   .ForMember(x => x.Question4, m => m.MapFrom(a => a.Question4))
                   .ForMember(x => x.Question4Comment, m => m.MapFrom(a => a.Question4Comment))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ReverseMap();

                cfg.CreateMap<ExitFormEmployeeDeclaration, ExitFormEmployeeDeclarationViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                   .ForMember(x => x.Question1, m => m.MapFrom(a => a.Question1))
                   .ForMember(x => x.Question1Comment, m => m.MapFrom(a => a.Question1Comment))
                   .ForMember(x => x.Question2, m => m.MapFrom(a => a.Question2))
                   .ForMember(x => x.Question2Comment, m => m.MapFrom(a => a.Question2Comment))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ReverseMap();

                cfg.CreateMap<PersonExitProcessForm, ExitProcessFormViewModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.SeparationID, m => m.MapFrom(a => a.SeparationID))
                   .ForMember(x => x.FeedbackForLeavingOrg, m => m.MapFrom(a => a.FeedbackForLeavingOrg))
                   .ForMember(x => x.FeedbackForLeavingOrgRemark, m => m.MapFrom(a => a.FeedbackForLeavingOrgRemark))
                   .ForMember(x => x.RatingForReportingLead, m => m.MapFrom(a => a.RatingForReportingLead))
                   .ForMember(x => x.RatingForReportingLeadRemark, m => m.MapFrom(a => a.RatingForReportingLeadRemark))
                   .ForMember(x => x.RatingForOrganization, m => m.MapFrom(a => a.RatingForOrganization))
                   .ForMember(x => x.RatingForOrganizationRemark, m => m.MapFrom(a => a.RatingForOrganizationRemark))
                   .ForMember(x => x.OrganizationDevelopmentSuggestion, m => m.MapFrom(a => a.OrganizationDevelopmentSuggestion))
                   .ForMember(x => x.OrgDevelopmentSuggestionRemark, m => m.MapFrom(a => a.OrgDevelopmentSuggestionRemark))
                   .ForMember(x => x.EmployeeDeclaration, m => m.MapFrom(a => a.EmployeeDeclaration))
                   .ForMember(x => x.EmployeeDeclarationRemark, m => m.MapFrom(a => a.EmployeeDeclarationRemark))
                   .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                   .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                   .ForMember(x => x.UpdatedBy, m => m.MapFrom(a => a.UpdatedBy))
                   .ForMember(x => x.UpdatedOn, m => m.MapFrom(a => a.UpdatedOn))
                   .ForMember(x => x.IsHRReviewDone, m => m.MapFrom(a => a.IsHRReviewDone))
                   .ReverseMap();

                cfg.CreateMap<Designation, EmployeeDesignation>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Text,
                               m => m.MapFrom(a => a.Name))
                    .ReverseMap();

                cfg.CreateMap<Expense_New, NewExpenseViewModel>()
                  .ForMember(x => x.IsClientReimbursment, m => m.MapFrom(a => a.IsClientReimbursment))
                  .ForMember(x => x.expenseId, m => m.MapFrom(a => a.ExpenseId))
                  .ForMember(x => x.details, m => m.MapFrom(a => a.ExpenseDetails_New))
                  .ForMember(x => x.CurrencyId, m => m.MapFrom(a => a.Currency))
                  .ForMember(x => x.CostCenterId, m => m.MapFrom(a => a.CostCenter))
                  .ForMember(x => x.PrimaryApproverId, m => m.MapFrom(a => a.PrimaryApprover))
                  .ForMember(x => x.SecondaryApproverId, m => m.MapFrom(a => a.SecondaryApprover))
                  .ForMember(x => x.IsApproved, m => m.MapFrom(a => a.IsApproved))
                  .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.Person.ID))
                  .ForMember(x => x.FirstName, m => m.MapFrom(a => a.Person.FirstName.ToString()))
                  .ForMember(x => x.LastName, m => m.MapFrom(a => a.Person.LastName))
                  .ForMember(x => x.IsRejected, m => m.MapFrom(a => a.IsRejected))
                  .ForMember(x => x.RequestDate, m => m.MapFrom(a => a.CreatedDate))
                  .ForMember(x => x.advance, m => m.MapFrom(a => a.Advance))
                  .ForMember(x => x.amountReimbursed, m => m.MapFrom(a => a.Balance))
                  .ForMember(x => x.totalExpenses, m => m.MapFrom(a => a.TotalAmount))
                  .ForMember(x => x.stageId, m => m.MapFrom(a => a.StageID))
                  .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                  .ForMember(x => x.expenseApprovedDate, m => m.MapFrom(a => a.PrimaryApprovalOn))
                  .ForMember(x => x.financeApprovedDate, m => m.MapFrom(a => a.ReImbursedOn))
                  .ForMember(x => x.financeApproverId, m => m.MapFrom(a => a.FinanceApprover))
                  .ForMember(x => x.comments, m => m.MapFrom(a => a.Comment))
                  .ForMember(x => x.IsHold, m => m.MapFrom(a => a.IsHold))
                  .ReverseMap()
                  .ForMember(x => x.Currency, m => m.MapFrom(a => a.CurrencyId))
                  .ForMember(x => x.CostCenter, m => m.MapFrom(a => a.CostCenterId))
                  .ForMember(x => x.PrimaryApprover, m => m.MapFrom(a => a.PrimaryApproverId))
                  .ForMember(x => x.SecondaryApprover, m => m.MapFrom(a => a.SecondaryApproverId))
                  .ForMember(x => x.ExpenseDetails_New, m => m.MapFrom(a => a.details))
                  .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.RequestDate))
                  .ForMember(x => x.Advance, m => m.MapFrom(a => a.advance))
                  .ForMember(x => x.Balance, m => m.MapFrom(a => a.amountReimbursed))
                  .ForMember(x => x.TotalAmount, m => m.MapFrom(a => a.totalExpenses))
                  .ForMember(x => x.Comment, m => m.MapFrom(a => a.comments));

                cfg.CreateMap<ExpenseDetails_New, NewEmployeeExpenseDetails>()
                    .ForMember(x => x.Amount, m => m.MapFrom(a => a.Amount))
                    .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile))
                    .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                    .ForMember(x => x.ExpenseCategoryId, m => m.MapFrom(a => a.ExpenseCategoryId))
                    .ForMember(x => x.ExpenseDate, m => m.MapFrom(a => a.ExpenseDate))
                    .ForMember(x => x.ProjectId, m => m.MapFrom(a => a.ProjectId))
                    .ForMember(x => x.ExpenseDetailId, m => m.MapFrom(a => a.ExpenseDetailId))
                    .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile))
                    .ForMember(x => x.ReceiptNo, m => m.MapFrom(a => a.ReceiptNo))
                    .ReverseMap();

                cfg.CreateMap<GetAllActiveEmployee_Result, Pheonix.Models.ViewModels.AppraisalEmployeeViewModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.EmpID, m => m.MapFrom(a => a.PersonID))
                .ForMember(x => x.EmpName, m => m.MapFrom(a => a.EmpName))
                .ForMember(x => x.AppraiserId, m => m.MapFrom(a => a.AppraiserID))
                .ForMember(x => x.AppraiserName, m => m.MapFrom(a => a.AppraiserName))
                .ForMember(x => x.ReviewerId, m => m.MapFrom(a => a.ReviewerID))
                .ForMember(x => x.ReviewerName, m => m.MapFrom(a => a.ReviewerName))
                .ForMember(x => x.Grade, m => m.MapFrom(a => a.Grade))
                .ForMember(x => x.Location, m => m.MapFrom(a => a.Location))
                .ForMember(x => x.FreezedComment, m => m.MapFrom(a => a.FreezedComments))
                .ForMember(x => x.Designation, m => m.MapFrom(a => a.Designation))
                .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                .ReverseMap();

                cfg.CreateMap<WorkLocation, LocationListModel>()
                   .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.LocationName, m => m.MapFrom(a => a.LocationName))
                     .ForMember(x => x.ParentLocation, m => m.MapFrom(a => a.ParentLocation))
                       .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                         .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                         .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
                   .ReverseMap();

                cfg.CreateMap<ExpenseCategory_New, ExpenseCategoryViewModel>()
                  .ForMember(x => x.ExpenseCategoryId, m => m.MapFrom(a => a.ExpenseCategoryId))
                  .ForMember(x => x.Title, m => m.MapFrom(a => a.Title))
                  .ForMember(x => x.LocationID, m => m.MapFrom(a => a.LocationID))
                  .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                  .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                  .ForMember(x => x.ExpenseCategoryId, m => m.MapFrom(a => a.CreatedOn))
                  .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                  .ReverseMap();

                cfg.CreateMap<PMSInvoice, InvoiceViewModel>()
                   .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                   .ForMember(x => x.Project, m => m.MapFrom(a => a.Project))
                   .ForMember(x => x.Category, m => m.MapFrom(a => a.Category))
                   .ForMember(x => x.Type, m => m.MapFrom(a => a.Type))
                   .ForMember(x => x.CreditDays, m => m.MapFrom(a => a.CreditDays))
                   .ForMember(x => x.Currency, m => m.MapFrom(a => a.Currency))
                   .ForMember(x => x.ContactPerson, m => m.MapFrom(a => a.ContactPerson))
                   .ForMember(x => x.ContactEmail, m => m.MapFrom(a => a.ContactEmail))
                   .ForMember(x => x.Contract, m => m.MapFrom(a => a.Contract))
                   .ForMember(x => x.CustomerAddress, m => m.MapFrom(a => a.CustomerAddress))
                   .ForMember(x => x.SalesPeriod, m => m.MapFrom(a => a.SalesPeriod))
                   .ForMember(x => x.TotalAmt, m => m.MapFrom(a => a.TotalAmt))
                   .ForMember(x => x.TotalDiscount, m => m.MapFrom(a => a.TotalDiscount))
                   .ForMember(x => x.NetAmount, m => m.MapFrom(a => a.NetAmount))
                   .ForMember(x => x.IsDraft, m => m.MapFrom(a => a.IsDraft))
                   .ForMember(x => x.IsRejected, m => m.MapFrom(a => a.IsRejected))
                   .ForMember(x => x.RaisedBy, m => m.MapFrom(a => a.RaisedBy))
                   .ForMember(x => x.ApprovedBy, m => m.MapFrom(a => a.ApprovedBy))
                   .ForMember(x => x.InvoiceDetailsModel, m => m.MapFrom(a => a.PMSInvoiceDetails))
                   .ForMember(x => x.StageId, m => m.MapFrom(a => a.StageId))
                   .ForMember(x => x.PrimaryApprovalOn, m => m.MapFrom(a => a.PrimaryApprovalOn))
                   .ForMember(x => x.PrimaryApprover, m => m.MapFrom(a => a.PrimaryApprover))
                   .ForMember(x => x.InvoiceFormCode, m => m.MapFrom(a => a.InvoiceFormCode))
                   .ForMember(x => x.Details, m => m.MapFrom(a => a.Details))
                   .ReverseMap()
                   .ForMember(x => x.PMSInvoiceDetails, m => m.MapFrom(a => a.InvoiceDetailsModel));

                cfg.CreateMap<PMSInvoiceDetails, InvoiceDetailsViewModel>()
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.Quantity, m => m.MapFrom(a => a.Quantity))
                    .ForMember(x => x.Rate, m => m.MapFrom(a => a.Rate.HasValue ? a.Rate.Value : 0))
                    .ForMember(x => x.Amount, m => m.MapFrom(a => a.Amount.HasValue ? a.Amount.Value : 0))
                    .ForMember(x => x.Discount, m => m.MapFrom(a => a.Discount.HasValue ? a.Discount.Value : 0))
                    .ForMember(x => x.NetAmount, m => m.MapFrom(a => a.NetAmount.HasValue ? a.NetAmount.Value : 0))
                    .ForMember(x => x.BillableRsrc, m => m.MapFrom(a => a.BillableRsrc))
                    .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.InvoiceID, m => m.MapFrom(a => a.InvoiceID))
                    .ForMember(x => x.SOW_Reference, m => m.MapFrom(a => a.SOW_ReferenceValue))
                    .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile))
                    .ReverseMap()
                    .ForMember(x => x.SOW_Reference, m => m.MapFrom(a => a.SOW_Reference.Any() ? string.Join(",", a.SOW_Reference) : string.Empty));

                cfg.CreateMap<PMSInvoicePayments, InvoicePaymentsViewModel>()
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.InvoiceId, m => m.MapFrom(a => a.InvoiceId))
                    .ForMember(x => x.PaymentDate, m => m.MapFrom(a => a.PaymentDate))
                    .ForMember(x => x.PaymentRecieved, m => m.MapFrom(a => a.PaymentRecieved))
                    .ForMember(x => x.BalAmt, m => m.MapFrom(a => a.BalAmt))
                    .ReverseMap();

                cfg.CreateMap<ProjectSkillViewModel, ProjectSkill>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                .ForMember(x => x.ProjectID, m => m.MapFrom(a => a.ProjectID))
                .ForMember(x => x.SkillID, m => m.MapFrom(a => a.SkillID))
                .ReverseMap();

                cfg.CreateMap<vm.PMSRolesViewModel, PMSRoles>()
               .ReverseMap();
                cfg.CreateMap<RoleViewModel, Role>().ReverseMap();
                cfg.CreateMap<vm.PMSActionViewModel, PMSAction>().ReverseMap();

                cfg.CreateMap<TARRFViewModel, TARRF>()
             .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
             .ForMember(x => x.PrimaryApproverComments, m => m.MapFrom(a => a.PrimaryApproverComments))
             .ForMember(x => x.PrimaryApprover, m => m.MapFrom(a => a.PrimaryApprover))
             .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
             .ForMember(x => x.CreatedDate, m => m.MapFrom(a => a.CreatedDate))
             .ForMember(x => x.Designation, m => m.MapFrom(a => a.Designation))
             .ForMember(x => x.DeliveryUnit, m => m.MapFrom(a => a.DeliveryUnit))
             .ForMember(x => x.EmploymentType, m => m.MapFrom(a => a.EmploymentType))
             .ForMember(x => x.ExpectedClosureDate, m => m.MapFrom(a => a.ExpectedClosureDate))
             .ForMember(x => x.HRApprover, m => m.MapFrom(a => a.HRApprover))
             .ForMember(x => x.HRApproverComments, m => m.MapFrom(a => a.HRApproverComments))
             .ForMember(x => x.JD, m => m.MapFrom(a => a.JD))
             .ForMember(x => x.MaxYrs, m => m.MapFrom(a => a.MaxYrs))
             .ForMember(x => x.MinYrs, m => m.MapFrom(a => a.MinYrs))
             .ForMember(x => x.ModifiedBy, m => m.MapFrom(a => a.ModifiedBy))
             .ForMember(x => x.ModifiedDate, m => m.MapFrom(a => a.ModifiedDate))
             .ForMember(x => x.Position, m => m.MapFrom(a => a.Position))
             .ForMember(x => x.RequestDate, m => m.MapFrom(a => a.RequestDate))
             .ForMember(x => x.RequestorComments, m => m.MapFrom(a => a.RequestorComments))
             .ForMember(x => x.Requestor, m => m.MapFrom(a => a.Requestor))
             .ForMember(x => x.RRFNo, m => m.MapFrom(a => a.RRFNo))
             .ForMember(x => x.RRFStatus, m => m.MapFrom(a => a.RRFStatus))
             .ForMember(x => x.IsDraft, m => m.MapFrom(a => a.IsDraft))
             .ForMember(x => x.SLA, m => m.MapFrom(a => a.SLA))
             .ReverseMap();

                cfg.CreateMap<TARRFDetailViewModel, TARRFDetail>()
                .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                .ForMember(x => x.RRFNo, m => m.MapFrom(a => a.RRFNo))
                .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                .ForMember(x => x.comments, m => m.MapFrom(a => a.comments))
                .ForMember(x => x.RRFNumber, m => m.MapFrom(a => a.RRFNumber))
                .ReverseMap();

                cfg.CreateMap<ContractAttachmentViewModel, ContractAttachments>()
                    .ForMember(x => x.Id, m => m.MapFrom(a => a.Id))
                    .ForMember(x => x.ContractId, m => m.MapFrom(a => a.ContractId))
                    .ForMember(x => x.ContractFileName, m => m.MapFrom(a => a.ContractFileName))
                    .ReverseMap();

             //   cfg.CreateMap<BGParameterList, AdminBGCConfigModel>()
             //     .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
             //     .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
             //     .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
             //     .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
             //     .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
             //.ReverseMap();

                cfg.CreateMap<ValuePortal.VPBenefit, Pheonix.Models.ViewModels.VPBenefitViewModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Name,
                              m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.Description,
                              m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.IsActive,
                              m => m.MapFrom(a => a.IsActive))
                   .ReverseMap();

                cfg.CreateMap<ValuePortal.VPCost, Pheonix.Models.ViewModels.VPCostViewModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Name,
                              m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.Description,
                              m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.IsActive,
                              m => m.MapFrom(a => a.IsActive))
                   .ReverseMap();

                cfg.CreateMap<ValuePortal.VPPriority, Pheonix.Models.ViewModels.VPPriorityViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Description,
                               m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.IsActive,
                               m => m.MapFrom(a => a.IsActive))
                    .ReverseMap();

                cfg.CreateMap<ValuePortal.VPConfiguration, Pheonix.Models.ViewModels.VPConfigurationViewModel>()
                  .ForMember(x => x.ID,
                             m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.Name,
                             m => m.MapFrom(a => a.Name))
                  .ForMember(x => x.Description,
                             m => m.MapFrom(a => a.Description))
                  .ForMember(x => x.IsActive,
                             m => m.MapFrom(a => a.IsActive))
                  .ReverseMap();

                cfg.CreateMap<ValuePortal.VPStatu, Pheonix.Models.ViewModels.VPStatusViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name,
                               m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Description,
                               m => m.MapFrom(a => a.Description))
                    .ForMember(x => x.IsActive,
                               m => m.MapFrom(a => a.IsActive))
                    .ReverseMap();

                cfg.CreateMap<VCFApprover, VCFApproverModel>()
                  .ForMember(x => x.id, m => m.MapFrom(a => a.id))
                  .ForMember(x => x.DeliveryUnitID, m => m.MapFrom(a => a.DeliveryUnitID))
                  .ForMember(x => x.ReviewerId, m => m.MapFrom(a => a.ReviewerId))
                  .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                  .ReverseMap();

                cfg.CreateMap<ValuePortal.VPPriority, Pheonix.Models.ViewModels.VPPriorityViewModel>()
                   .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID));

                cfg.CreateMap<ValuePortal.VPComment, Pheonix.Models.ViewModels.VPCommentsViewModel>()
                  .ForMember(x => x.id,
                             m => m.MapFrom(a => a.id))
                  .ForMember(x => x.ReviewerComments,
                             m => m.MapFrom(a => a.ReviewerComments))
                  .ForMember(x => x.ReviewerId,
                             m => m.MapFrom(a => a.ReviewerId))
                  .ForMember(x => x.VPIdeaDetailID,
                             m => m.MapFrom(a => a.VPIdeaDetailID))
                  .ReverseMap();

                cfg.CreateMap<VCFConfigurationList, VCFListModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                 .ForMember(x => x.Description, m => m.MapFrom(a => a.Description));

                cfg.CreateMap<ValuePortal.VPIdeaDetail, Pheonix.Models.ViewModels.VCIdeaMasterViewModel>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.IdeaHeadline,
                               m => m.MapFrom(a => a.IdeaHeadline))
                    .ForMember(x => x.IdeaDescription,
                               m => m.MapFrom(a => a.IdeaDescription))
                    .ForMember(x => x.IdeaBenefits,
                               m => m.MapFrom(a => a.IdeaBenefits))
                    .ForMember(x => x.RequiredEffort,
                               m => m.MapFrom(a => a.RequiredEffort))
                    .ForMember(x => x.RequiredResources,
                               m => m.MapFrom(a => a.RequiredResources))
                    .ForMember(x => x.RequiredTechnologies,
                               m => m.MapFrom(a => a.RequiredTechnologies))
                    .ForMember(x => x.SubmittedBy,
                               m => m.MapFrom(a => a.SubmittedBy))
                    .ForMember(x => x.ExecutionApproach,
                               m => m.MapFrom(a => a.ExecutionApproach))
                    .ForMember(x => x.IsEmailReceiptRequired,
                               m => m.MapFrom(a => a.IsEmailReceiptRequired))
                    .ForMember(x => x.StatusID,
                               m => m.MapFrom(a => a.StatusID))
                    .ForMember(x => x.PriorityID,
                               m => m.MapFrom(a => a.PriorityID))
                    .ForMember(x => x.IsDeleted,
                               m => m.MapFrom(a => a.IsDeleted))
                    .ForMember(x => x.VPComments,
                               m => m.MapFrom(a => a.VPComments))
                    .ForMember(x => x.BenefitFactor,
                               m => m.MapFrom(a => a.BenefitFactor))
                     .ForMember(x => x.BenefitScope,
                               m => m.MapFrom(a => a.BenefitScope))
                    .ReverseMap();

                cfg.CreateMap<DeliveryTeam, DeliveryTeamModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Code, m => m.MapFrom(a => a.Code))
                 .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                 .ForMember(x => x.IsActive, m => m.MapFrom(a => a.IsActive))
                 .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                 .ReverseMap();

                cfg.CreateMap<ResourcePool, ResourcePoolModel>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                  .ForMember(x => x.Code, m => m.MapFrom(a => a.Code))
                  .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                  .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                  .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                  .ReverseMap();

                cfg.CreateMap<PMSTaskTypes, TaskTypeModel>()
                 .ForMember(x => x.id, m => m.MapFrom(a => a.Id))
                 .ForMember(x => x.typeName, m => m.MapFrom(a => a.TypeName))
                 .ForMember(x => x.parentTaskId, m => m.MapFrom(a => a.ParentTaskId))
                 .ReverseMap();


                //Obj. Mapping for VCF Idea list & Idea Details page for fetch limited data from API.
                //(Mapping of Entity Obj : VPIdeaDetail => ViewModel Obj : LimitedDataIdeaDetailsViewModel).
                cfg.CreateMap<ValuePortal.VPIdeaDetail, Pheonix.Models.ViewModels.LimitedDataIdeaDetailsViewModel>()
                  .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.IdeaHeadline, m => m.MapFrom(a => a.IdeaHeadline))
                 .ForMember(x => x.IdeaDescription, m => m.MapFrom(a => a.IdeaDescription))
                 .ForMember(x => x.RequiredEffort, m => m.MapFrom(a => a.RequiredEffort))
                // .ForMember(x => x.RequiredResources, m => m.MapFrom(a => a.RequiredResources))
                 .ForMember(x => x.RequiredTechnologies, m => m.MapFrom(a => a.RequiredTechnologies))
                 .ForMember(x => x.SubmittedBy, m => m.MapFrom(a => a.SubmittedBy))
                 .ForMember(x => x.TeammemberIds, m => m.MapFrom(a => a.TeammemberIds))
                // .ForMember(x => x.TeammemberNames, m => m.MapFrom(a => a.TeammemberNames))
                 .ForMember(x => x.VPComments, m => m.MapFrom(a => a.VPComments))
                 .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                 .ForMember(x => x.StatusID, m => m.MapFrom(a => a.StatusID))
                 .ForMember(x => x.BusinessUnit, m => m.MapFrom(a => a.BusinessUnit))
                 .ForMember(x => x.BenefitFactor, m => m.MapFrom(a => a.BenefitFactor))
                 .ForMember(x => x.BenefitScope, m => m.MapFrom(a => a.BenefitScope))
                 .ReverseMap();

                //Obj. Mapping for VCF Admin section for Global Approver CRD Screen. 
                //(Mapping of Entity Obj : PersonInRole => ViewModel Obj : VCFGlobalApproverModel).
                cfg.CreateMap<PersonInRole, VCFGlobalApproverModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                 .ForMember(x => x.RoleID, m => m.MapFrom(a => a.RoleID))
                 .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                 .ReverseMap();

                cfg.CreateMap<ValuePortal.VPBenefitScope, Pheonix.Models.ViewModels.VPBenefitScopeViewModel>()
                   .ForMember(x => x.ID,
                              m => m.MapFrom(a => a.ID))
                   .ForMember(x => x.Name,
                              m => m.MapFrom(a => a.Name))
                   .ForMember(x => x.Description,
                              m => m.MapFrom(a => a.Description))
                   .ForMember(x => x.IsActive,
                              m => m.MapFrom(a => a.IsActive))
                   .ReverseMap();

                cfg.CreateMap<GetAllReportsByPersonId_Result, Pheonix.Models.ViewModels.ReportAccessConfigViewModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                 .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                 .ForMember(x => x.RedirectionText, m => m.MapFrom(a => a.RedirectionText))
                 .ForMember(x => x.ReportHeaderText, m => m.MapFrom(a => a.ReportHeaderText))
                 .ForMember(x => x.SelectedReportText, m => m.MapFrom(a => a.SelectedReportText))
                 .ForMember(x => x.ImageUrl, m => m.MapFrom(a => a.ImageUrl))
                 .ForMember(x => x.IsActive, m => m.MapFrom(a => a.IsActive))
                 .ForMember(x => x.DefaultToAll, m => m.MapFrom(a => a.DefaultToAll))
                 .ForMember(x => x.ParentReportID, m => m.MapFrom(a => a.ParentReportID))
                 .ForMember(x => x.RouteUrl, m => m.MapFrom(a => a.RouteUrl))
                 .ReverseMap();

                cfg.CreateMap<SkillMatrix, SkillsModel>()
                    .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.Name, m => m.MapFrom(a => a.Name))
                    .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
                    .ForMember(x => x.SkillCategory, m => m.MapFrom(a => a.SkillCategory))
                    .ForMember(x => x.IsDeleted, m => m.MapFrom(a => a.IsDeleted))
                    .ReverseMap();

                cfg.CreateMap<GetAppraisalReportByYear_Result, Pheonix.Models.ViewModels.AppraisalReportViewModel>()
                     .ForMember(x => x.EmpId, m => m.MapFrom(a => a.PersonID))
                     .ForMember(x => x.EmpName, m => m.MapFrom(a => a.EmpName))
                     .ForMember(x => x.Status, m => m.MapFrom(a => a.Status))
                     .ForMember(x => x.Year, m => m.MapFrom(a => a.AppraiserYear))
                     .ForMember(x => x.AppraiserComments, m => m.MapFrom(a => a.AppraiserComments))
                     .ForMember(x => x.ReviewerComments, m => m.MapFrom(a => a.ReviewerComments))
                     .ForMember(x => x.OneToOneComments, m => m.MapFrom(a => a.OneToOneComment))
                     .ForMember(x => x.AppraiserRating, m => m.MapFrom(a => a.AppraiserRating))
                     .ForMember(x => x.ReviewerRating, m => m.MapFrom(a => a.ReviewerRating))
                     .ForMember(x => x.Grade, m => m.MapFrom(a => a.Grade))
                     .ForMember(x => x.IsPromotion, m => m.MapFrom(a => a.IsPromotion))
                     .ForMember(x => x.AppraiserName, m => m.MapFrom(a => a.AppraiserName))
                     .ForMember(x => x.ReviewerName, m => m.MapFrom(a => a.ReviewerName))
                     .ForMember(x => x.Location, m => m.MapFrom(a => a.Location))
                     .ForMember(x => x.AppraiserRatingBySystem, m => m.MapFrom(a => a.AppraiserRatingBySystem))
                     .ForMember(x => x.ReviewerRatingBySystem, m => m.MapFrom(a => a.ReviewerRatingBySystem))
                     .ForMember(x => x.IsPromotionByRiviwer, m => m.MapFrom(a => a.IsPromotionByRiviwer))
                     .ForMember(x => x.PromotionForByRiviwer, m => m.MapFrom(a => a.PromotionForByRiviwer))
                     .ForMember(x => x.PromotionFor, m => m.MapFrom(a => a.PromotionFor))
                     .ForMember(x => x.IsTrainingRequired, m => m.MapFrom(a => a.IsTrainingRequired))
                     .ForMember(x => x.TrainingFor, m => m.MapFrom(a => a.TrainingFor))
                     .ForMember(x => x.IsCriticalForOrganize, m => m.MapFrom(a => a.IsCriticalForOrganize))
                     .ForMember(x => x.CriticalForOrganizeFor, m => m.MapFrom(a => a.CriticalForOrganizeFor))
                     .ForMember(x => x.IsCriticalForProject, m => m.MapFrom(a => a.IsCriticalForProject))
                     .ForMember(x => x.CriticalForProject, m => m.MapFrom(a => a.CriticalForProject))
                     .ForMember(x => x.IsPromotionNorm, m => m.MapFrom(a => a.IsPromotionNorm))
                     .ForMember(x => x.PromotionforByNorm, m => m.MapFrom(a => a.PromotionforByNorm))
                     .ForMember(x => x.OrganizationCategory, m => m.MapFrom(a => a.organizationCategory))
                     .ForMember(x => x.OrganizationComment, m => m.MapFrom(a => a.OrganizationComment))
                     .ForMember(x => x.LocationId, m => m.MapFrom(a => a.LocationId))
                     .ReverseMap();
           

            cfg.CreateMap<LeavesCreditViewModel, PersonLeaveCredit>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Narration,
                               m => m.MapFrom(a => a.Narration))
                    .ForMember(x => x.CreditBalance,
                               m => m.MapFrom(a => a.CreditBalance))
                    .ForMember(x => x.CreditedBy,
                               m => m.MapFrom(a => a.CreditedBy))
                    .ForMember(x => x.Year,
                               m => m.MapFrom(a => a.Year))
                    .ForMember(x => x.DateEffective,
                               m => m.MapFrom(a => a.DateEffective));


                cfg.CreateMap<LeavesCreditViewModel, PersonCLCredit>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Narration,
                               m => m.MapFrom(a => a.Narration))
                    .ForMember(x => x.CreditBalance,
                               m => m.MapFrom(a => a.CreditBalance))
                    .ForMember(x => x.CreditedBy,
                               m => m.MapFrom(a => a.CreditedBy))
                    .ForMember(x => x.Year,
                               m => m.MapFrom(a => a.Year))
                    .ForMember(x => x.DateEffective,
                               m => m.MapFrom(a => a.DateEffective));

                cfg.CreateMap<LeavesCreditViewModel, PersonSLCredit>()
                    .ForMember(x => x.ID,
                               m => m.MapFrom(a => a.ID))
                    .ForMember(x => x.PersonID,
                               m => m.MapFrom(a => a.PersonID))
                    .ForMember(x => x.Narration,
                               m => m.MapFrom(a => a.Narration))
                    .ForMember(x => x.CreditBalance,
                               m => m.MapFrom(a => a.CreditBalance))
                    .ForMember(x => x.CreditedBy,
                               m => m.MapFrom(a => a.CreditedBy))
                    .ForMember(x => x.Year,
                               m => m.MapFrom(a => a.Year))
                    .ForMember(x => x.DateEffective,
                               m => m.MapFrom(a => a.DateEffective));

                cfg.CreateMap<vm.KRAProgressViewModel, KRAProgress>()
                 .ForMember(x => x.VibrantName, m => m.MapFrom(a => a.VibrantName))
                 .ForMember(x => x.Comment, m => m.MapFrom(a => a.Comment))
                 .ForMember(x => x.KRAGoalId, m => m.MapFrom(a => a.KRAGoalId));

                cfg.CreateMap<vm.PersonKRAViewModel, PersonKRA>().
                ForMember(x => x.KRAInitiationId, m => m.MapFrom(a => a.PersonKRAInitiationId)).
                ForMember(x => x.KRAYearId, m => m.MapFrom(a => a.KRAYearId)).
                ForMember(x => x.PersonId, m => m.MapFrom(a => a.PersonId)).
                ForMember(x => x.Q1, m => m.MapFrom(a => a.Q1)).
                ForMember(x => x.Q2, m => m.MapFrom(a => a.Q2)).
                ForMember(x => x.Q3, m => m.MapFrom(a => a.Q3)).
                ForMember(x => x.Q4, m => m.MapFrom(a => a.Q4)).
                ForMember(x => x.ReviewerPersonId, m => m.MapFrom(a => a.ReviewerPersonId)).
                ForMember(x => x.FirstKRAInitiatedBy, m => m.MapFrom(a => a.FirstKRAInitiatedBy));

                cfg.CreateMap<vm.PersonKRADetailViewModel, PersonKRADetail>().
                ForMember(x => x.Id, m => m.MapFrom(a => a.Id)).
                ForMember(x => x.IsCloned, m => m.MapFrom(a => a.IsCloned)).
                ForMember(x => x.IsCompleted, m => m.MapFrom(a => a.IsCompleted)).
                ForMember(x => x.IsDelete, m => m.MapFrom(a => a.IsDelete)).
                ForMember(x => x.IsEmployeeEdit, m => m.MapFrom(a => a.IsEmployeeEdit)).
                ForMember(x => x.IsKRAAvailableForClone, m => m.MapFrom(a => a.IsKRAAvailableForClone)).
                ForMember(x => x.IsKRADone, m => m.MapFrom(a => a.IsKRADone)).
                ForMember(x => x.IsKRAIntitiationCompleted, m => m.MapFrom(a => a.IsKRAIntitiationCompleted)).
                ForMember(x => x.IsKRAQuarterCompleted, m => m.MapFrom(a => a.IsKRAQuarterCompleted)).
                ForMember(x => x.IsManagerEdit, m => m.MapFrom(a => a.IsManagerEdit)).
                ForMember(x => x.IsValid, m => m.MapFrom(a => a.IsValid)).
                ForMember(x => x.KRA, m => m.MapFrom(a => a.KRA)).
                ForMember(x => x.KRACategoryId, m => m.MapFrom(a => a.KRACategoryId)).
                ForMember(x => x.KRADoneOn, m => m.MapFrom(a => a.KRADoneOn)).
                ForMember(x => x.KRAEndDate, m => m.MapFrom(a => a.KRAEndDate)).
                ForMember(x => x.KRAInitiationEndDate, m => m.MapFrom(a => a.KRAInitiationEndDate)).
                ForMember(x => x.KRAInitiationId, m => m.MapFrom(a => a.KRAInitiationId)).
                ForMember(x => x.KRAPercentageCompletion, m => m.MapFrom(a => a.KRAPercentageCompletion)).
                ForMember(x => x.KRAStartDate, m => m.MapFrom(a => a.KRAStartDate)).
                ForMember(x => x.ModifiedBy, m => m.MapFrom(a => a.ModifiedBy)).
                ForMember(x => x.ModifiedOn, m => m.MapFrom(a => a.ModifiedOn)).
                ForMember(x => x.ParentKRAId, m => m.MapFrom(a => a.ParentKRAId)).
                ForMember(x => x.PersonId, m => m.MapFrom(a => a.PersonId)).
                ForMember(x => x.Q1, m => m.MapFrom(a => a.Q1)).
                ForMember(x => x.Q2, m => m.MapFrom(a => a.Q2)).
                ForMember(x => x.Q3, m => m.MapFrom(a => a.Q3)).
                ForMember(x => x.Q4, m => m.MapFrom(a => a.Q4)).
                ForMember(x => x.Weightage, m => m.MapFrom(a => a.Weightage)).
                ForMember(x => x.YearId, m => m.MapFrom(a => a.YearId)).
                ForMember(x => x.KRAClonedBy, m => m.MapFrom(a => a.KRAClonedBy)).
                ForMember(x => x.KRAClonedDate, m => m.MapFrom(a => a.KRAClonedDate)).
                ForMember(x => x.KRAHistoryClonedBy, m => m.MapFrom(a => a.KRAHistoryClonedBy)).
                ForMember(x => x.KRAHistoryClonedDate, m => m.MapFrom(a => a.KRAHistoryClonedDate)).
                ForMember(x => x.KRAHistoryId, m => m.MapFrom(a => a.KRAHistoryId)).
                ForMember(x => x.IsClonedFromHistory, m => m.MapFrom(a => a.IsClonedFromHistory)).
                ForMember(x => x.KRAGoalId, m => m.MapFrom(a => a.KRAGoalId));

                cfg.CreateMap<KRAInitiationStatus_Result, vm.KRAInitiationDetail>()
                 .ForMember(x => x.IsKRAHistroryAvailable, m => m.MapFrom(a => a.IsKRAHistroryAvailable))
                 .ForMember(x => x.IsKRAInitiated, m => m.MapFrom(a => a.KRAInitiatedValue))
                 .ForMember(x => x.IsReviewer, m => m.MapFrom(a => a.ReviewerValue))
                 .ForMember(x => x.PersonIdList, m => m.MapFrom(a => a.PersonIdList));

                cfg.CreateMap<SearchAllKRAHistoryDetail_Result, Rpt_SearchAllKRADetail_Result>()
                .ForMember(x => x.ID, m => m.MapFrom(a => a.KRAId))
                .ForMember(x => x.KRACategoryId, m => m.MapFrom(a => a.KRACategoryId))
                .ForMember(x => x.PersonId, m => m.MapFrom(a => a.PersonId))
                .ForMember(x => x.EmployeeName, m => m.MapFrom(a => a.EmployeeName))
                .ForMember(x => x.KRA, m => m.MapFrom(a => a.KRA))
                .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                .ForMember(x => x.YearId, m => m.MapFrom(a => a.YearId))
                .ForMember(x => x.Weightage, m => m.MapFrom(a => a.Weightage))
                .ForMember(x => x.IsValid, m => m.MapFrom(a => a.IsValid))
                .ForMember(x => x.Q1, m => m.MapFrom(a => a.Q1))
                .ForMember(x => x.Q2, m => m.MapFrom(a => a.Q2))
                .ForMember(x => x.Q3, m => m.MapFrom(a => a.Q3))
                .ForMember(x => x.Q4, m => m.MapFrom(a => a.Q4))
                .ForMember(x => x.IsCloned, m => m.MapFrom(a => a.IsCloned))
                .ForMember(x => x.IsKRADone, m => m.MapFrom(a => a.IsKRADone))
                .ForMember(x => x.KRADoneOn, m => m.MapFrom(a => a.KRADoneOn))
                .ForMember(x => x.KRAPercentageCompletion, m => m.MapFrom(a => a.KRAPercentageCompletion))
                .ForMember(x => x.KRAStartDate, m => m.MapFrom(a => a.KRAStartDate))
                .ForMember(x => x.KRAEndDate, m => m.MapFrom(a => a.KRAEndDate))
                .ForMember(x => x.CreatedBY, m => m.MapFrom(a => a.CreatedBY))
                .ForMember(x => x.ModifiedBy, m => m.MapFrom(a => a.ModifiedBy))
                .ForMember(x => x.ReviewerPersonId, m => m.MapFrom(a => a.ReviewerPersonId))
                .ForMember(x => x.ReviewerName, m => m.MapFrom(a => a.ReviewerName));

                cfg.CreateMap<KRAReportViewModel, Rpt_SearchAllKRADetail_Result>()
                .ForMember(x => x.KRACategoryId, m => m.MapFrom(a => a.KRACategoryId))
                .ForMember(x => x.PersonId, m => m.MapFrom(a => a.PersonId))
                .ForMember(x => x.EmployeeName, m => m.MapFrom(a => a.EmployeeName))
                .ForMember(x => x.KRA, m => m.MapFrom(a => a.KRA))
                .ForMember(x => x.Description, m => m.MapFrom(a => a.Description))
                .ForMember(x => x.YearId, m => m.MapFrom(a => a.YearId))
                .ForMember(x => x.IsValid, m => m.MapFrom(a => a.IsValid))
                .ForMember(x => x.Q1, m => m.MapFrom(a => a.Q1))
                .ForMember(x => x.Q2, m => m.MapFrom(a => a.Q2))
                .ForMember(x => x.Q3, m => m.MapFrom(a => a.Q3))
                .ForMember(x => x.Q4, m => m.MapFrom(a => a.Q4))
                .ForMember(x => x.IsCloned, m => m.MapFrom(a => a.IsCloned))
                .ForMember(x => x.IsKRADone, m => m.MapFrom(a => a.IsKRADone))
                .ForMember(x => x.KRADoneOn, m => m.MapFrom(a => a.KRADoneOn))
                .ForMember(x => x.KRAPercentageCompletion, m => m.MapFrom(a => a.KRAPercentageCompletion))
                .ForMember(x => x.KRAStartDate, m => m.MapFrom(a => a.KRAStartDate))
                .ForMember(x => x.KRAEndDate, m => m.MapFrom(a => a.KRAEndDate))
                .ForMember(x => x.CreatedBY, m => m.MapFrom(a => a.CreatedBY))
                .ForMember(x => x.ModifiedBy, m => m.MapFrom(a => a.ModifiedBy))
                .ForMember(x => x.ReviewerPersonId, m => m.MapFrom(a => a.ReviewerPersonId))
                .ForMember(x => x.ReviewerName, m => m.MapFrom(a => a.ReviewerName)).ReverseMap();

                cfg.CreateMap<GetSeparationListData_Result, Pheonix.Models.ViewModels.SeperationViewModel>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.ID))
                 .ForMember(x => x.CreatedBy, m => m.MapFrom(a => a.CreatedBy))
                 .ForMember(x => x.CreatedOn, m => m.MapFrom(a => a.CreatedOn))
                 .ForMember(x => x.UpdatedBy, m => m.MapFrom(a => a.UpdatedBy))
                 .ForMember(x => x.UpdatedOn, m => m.MapFrom(a => a.UpdatedOn))
                 .ForMember(x => x.ResignDate, m => m.MapFrom(a => a.ResignDate))
                 .ForMember(x => x.ExpectedDate, m => m.MapFrom(a => a.ExpectedDate))
                 .ForMember(x => x.ActualDate, m => m.MapFrom(a => a.ActualDate))
                 .ForMember(x => x.SeperationReason, m => m.MapFrom(a => a.SeperationReason))
                 .ForMember(x => x.Comments, m => m.MapFrom(a => a.Comments))
                 .ForMember(x => x.ApprovalID, m => m.MapFrom(a => a.ApprovalID))
                 .ForMember(x => x.ApprovalDate, m => m.MapFrom(a => a.ApprovalDate))
                 .ForMember(x => x.StatusID, m => m.MapFrom(a => a.StatusID))
                 .ForMember(x => x.PersonID, m => m.MapFrom(a => a.PersonID))
                 .ForMember(x => x.NoticePeriod, m => m.MapFrom(a => a.NoticePeriod))
                 .ForMember(x => x.RejectRemark, m => m.MapFrom(a => a.RejectRemark))
                 .ForMember(x => x.isApprovedByHR, m => m.MapFrom(a => a.IsApprovedByHR))
                 .ForMember(x => x.WithdrawRemark, m => m.MapFrom(a => a.WithdrawRemark))
                 .ForMember(x => x.IsTermination, m => m.MapFrom(a => a.IsTermination))
                 .ForMember(x => x.TerminationRemark, m => m.MapFrom(a => a.TerminationRemark))
                 .ForMember(x => x.TerminationReason, m => m.MapFrom(a => a.TerminationReason))
                 .ForMember(x => x.AttachedFile, m => m.MapFrom(a => a.AttachedFile))
                 .ForMember(x => x.isWithdraw, m => m.MapFrom(a => a.IsWithdraw))
                 .ForMember(x => x.TotalLeaves, m => m.MapFrom(a => a.TotalLeaves))
                 .ForMember(x => x.LeavesTaken, m => m.MapFrom(a => a.LeavesTaken))
                 .ForMember(x => x.CompOff, m => m.MapFrom(a => a.CompOff))
                 .ForMember(x => x.LeavesAvailable, m => m.MapFrom(a => a.LeavesAvailable))
                 .ForMember(x => x.LeavesApplied, m => m.MapFrom(a => a.LeavesApplied))
                 .ForMember(x => x.CompOffAvailable, m => m.MapFrom(a => a.CompOffAvailable))
                 .ForMember(x => x.LWP, m => m.MapFrom(a => a.Lwp))
                 .ForMember(x => x.CompOffConsumed, m => m.MapFrom(a => a.CompOffConsumed))
                 .ForMember(x => x.ExitDateRemark, m => m.MapFrom(a => a.ExitDateRemark))
                 .ForMember(x => x.EmailID, m => m.MapFrom(a => a.EmailID.HasValue ? a.EmailID.Value.ToString() : null))
                 .ForMember(x => x.LWPDate, m => m.MapFrom(a => a.LwpDate))
                 .ForMember(x => x.ResignationWOSettlement, m => m.MapFrom(a => a.ResignationWOSettlement.HasValue ? a.ResignationWOSettlement.Value.ToString() : null))
                 .ForMember(x => x.OldEmploymentStatus, m => m.MapFrom(a => a.OldEmploymentStatus))
                 .ForMember(x => x.EmployeeProfile, m => m.MapFrom(src => Mapper.Map<GetSeparationListData_Result, EmployeeBasicProfile>(src)))
                 .ReverseMap();

                cfg.CreateMap<GetSeparationListData_Result, EmployeeBasicProfile>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.id))
                 .ForMember(x => x.FirstName, m => m.MapFrom(a => a.FirstName))
                 .ForMember(x => x.MiddleName, m => m.MapFrom(a => a.MiddleName))
                 .ForMember(x => x.LastName, m => m.MapFrom(a => a.LastName))
                 .ForMember(x => x.CurrentDesignationID, m => m.MapFrom(a => a.CurrentDesignationID.HasValue ? a.CurrentDesignationID.Value.ToString() : null))
                 .ForMember(x => x.CurrentDesignation, m => m.MapFrom(a => a.CurrentDesignation))
                 .ForMember(x => x.PFNo, m => m.MapFrom(a => a.PFNo))
                 .ForMember(x => x.PANNo, m => m.MapFrom(a => a.PANNo))
                 .ForMember(x => x.Passport, m => m.MapFrom(a => a.PassPort.HasValue ? a.PassPort.Value.ToString() : null))
                 .ForMember(x => x.ImagePath, m => m.MapFrom(a => a.ImagePath))
                 .ForMember(x => x.Email, m => m.MapFrom(a => a.Email))
                 .ForMember(x => x.Mobile, m => m.MapFrom(a => a.Mobile))
                 .ForMember(x => x.OL, m => m.MapFrom(a => a.OL))
                 .ForMember(x => x.OLText, m => m.MapFrom(a => a.OLText))
                 .ForMember(x => x.OfficeLocation, m => m.MapFrom(a => a.OfficeLocation.HasValue ? a.OfficeLocation.Value.ToString() : null))
                 .ForMember(x => x.SeatingLocation, m => m.MapFrom(a => a.SeatingLocation))
                 .ForMember(x => x.ResidenceNumber, m => m.MapFrom(a => a.ResidenceNumber))
                 .ForMember(x => x.Active, m => m.MapFrom(a => a.Active))
                 .ForMember(x => x.DateOfBirth, m => m.MapFrom(a => a.DateOfBirth))
                 .ForMember(x => x.LeavesRemaining, m => m.MapFrom(a => a.LeavesRemaining))
                 .ForMember(x => x.Extension, m => m.MapFrom(a => a.Extension))
                 .ForMember(x => x.joiningDate, m => m.MapFrom(a => a.JoiningDate))
                 .ForMember(x => x.probationReviewDate, m => m.MapFrom(a => a.ProbationReviewDate))
                 .ForMember(x => x.SkillName, m => m.MapFrom(a => a.SkillName.HasValue ? a.SkillName.Value.ToString() : null))
                 .ForMember(x => x.SkillRating, m => m.MapFrom(a => a.SkillRating))
                 .ForMember(x => x.ResourcePoolName, m => m.MapFrom(a => a.ResourcePoolName.HasValue ? a.ResourcePoolName.Value.ToString() : null))
                 .ForMember(x => x.DeliveryUnitName, m => m.MapFrom(a => a.DeliveryUnitName))
                 .ForMember(x => x.DeliveryTeamName, m => m.MapFrom(a => a.DeliveryTeamName))
                 .ForMember(x => x.exitDate, m => m.MapFrom(a => a.ExitDate))
                 .ForMember(x => x.CasualLeavesRemaining, m => m.MapFrom(a => a.CasualLeavesRemaining))
                 .ForMember(x => x.SickLeavesRemaining, m => m.MapFrom(a => a.SickLeavesRemaining))
                 .ForMember(x => x.EmployeementStaus, m => m.MapFrom(a => a.EmploymentStatus))
                 .ForMember(x => x.GenderID, m => m.MapFrom(a => a.GenderID))
                 .ReverseMap();

                cfg.CreateMap<vm.KRAFeedbackViewModel, PersonKRAFeedback>()
                 .ForMember(x => x.ID, m => m.MapFrom(a => a.Id))
                 .ForMember(x => x.KRAGoalId, m => m.MapFrom(a => a.KraGoalId))
                 .ForMember(x => x.Year, m => m.MapFrom(a => a.Year))
                 .ForMember(x => x.Quarter, m => m.MapFrom(a => a.Quarter))
                 .ForMember(x => x.PersonId, m => m.MapFrom(a => a.PersonId));
            });          
        }
    }
}