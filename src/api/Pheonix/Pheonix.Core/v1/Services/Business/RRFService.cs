using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class RRFService : IRRFService
    {
        private IBasicOperationsService service;

        public RRFService(IBasicOperationsService opsService)
        {
            service = opsService;
        }

        public async Task<IEnumerable<RRFReadOnlyViewModel>> GetRRFList(int userID, DateTime start, DateTime end)
        {
            return await Task.Run(() =>
            {
                var rrfList = service.Top<TARRF>(0, x => (x.Person.ID == userID) && x.RequestedDate.Value >= start.Date && x.ClosureDate.Value <= end.Date);
                var viewModel = Mapper.Map<IEnumerable<TARRF>, IEnumerable<RRFReadOnlyViewModel>>(rrfList);
                foreach (RRF rrf in rrfList)
                {
                    if (rrf.RRFSkill != null && rrf.RRFSkill.Count > 0)
                        viewModel.Where(a => a.ID == rrf.ID).FirstOrDefault().PrimarySkills = string.Join(",", rrf.RRFSkill.Where(b => b.Rating > 3).Select(p => p.SkillMatrix.Name));
                }
                return viewModel;
            });
        }

        public async Task<AddEditRRFViewModel> GetRRFDetails(int RRFId)
        {
            return await Task.Run(() =>
            {
                var editRRFViewModel = new AddEditRRFViewModel();
                var selectedRRF = service.First<RRF>(x => x.ID == RRFId);
                if (selectedRRF != null)
                {
                    editRRFViewModel.RRF = Mapper.Map<RRF, RRFViewModel>(selectedRRF);
                    editRRFViewModel.RRFSkill = Mapper.Map<List<RRFSkill>, List<RRFSkillsViewModel>>(selectedRRF.RRFSkill.ToList());
                    editRRFViewModel.RRFDetail = Mapper.Map<RRFDetail, RRFDetailsViewModels>(selectedRRF.RRFDetail.FirstOrDefault());
                    editRRFViewModel.RRFComments = Mapper.Map<List<RRFComments>, List<RRFCommentViewModel>>(selectedRRF.RRFComments.ToList());
                    return editRRFViewModel;
                }
                return null;
            });
        }

        public async Task<AddEditRRFViewModel> UpdateRRFDetails(AddEditRRFViewModel updatedRRFModel)
        {
            return await Task.Run(() =>
            {
                var isRRFUpdated = false;
                var isCommentUpdated = false;
                var selectedRRF = service.First<RRF>(x => x.ID == updatedRRFModel.RRF.ID);
                if (selectedRRF != null)
                {
                    var newRRFDetail = Mapper.Map<RRFDetailsViewModels, RRFDetail>(updatedRRFModel.RRFDetail);
                    isRRFUpdated = service.Update<RRFDetail>(newRRFDetail, selectedRRF.RRFDetail.FirstOrDefault());

                    var newRRF = Mapper.Map<RRFViewModel, RRF>(updatedRRFModel.RRF);
                    isRRFUpdated = service.Update<RRF>(newRRF, selectedRRF);

                    foreach (var skills in updatedRRFModel.RRFSkill)
                    {
                        if (skills.action == 1)
                        {
                            var newSkill = Mapper.Map<RRFSkillsViewModel, RRFSkill>(skills);
                            var isCreated = service.Create<RRFSkill>(newSkill, s => s.ID == newSkill.ID);
                        }
                        else if (skills.action == 2)
                        {
                            var oldSkill = service.First<RRFSkill>(x => x.ID == skills.ID);
                            var isDeleted = service.Remove<RRFSkill>(oldSkill, s => s.ID == skills.ID);
                        }
                    }

                    var newComments = Mapper.Map<RRFCommentViewModel, RRFComments>(updatedRRFModel.RRFComments.FirstOrDefault());
                    isCommentUpdated = service.Update<RRFComments>(newComments, selectedRRF.RRFComments.FirstOrDefault());

                    if (isRRFUpdated && isCommentUpdated)
                        service.Finalize(true);
                }
                return GetRRFDetails(updatedRRFModel.RRF.ID);
            });
        }

        public async Task<AddEditRRFViewModel> AddRRF(AddEditRRFViewModel updatedRRFModel)
        {
            return await Task.Run(() =>
            {
                var isRRFCreated = false;
                var isSkillCreated = false;
                var isCommentUpdated = false;

                var newRRF = Mapper.Map<RRFViewModel, RRF>(updatedRRFModel.RRF);
                isRRFCreated = service.Create<RRF>(newRRF, rrf => rrf.ID == newRRF.ID);

                if (isRRFCreated)
                    service.Finalize(true);

                newRRF = service.Top<RRF>(1, rrf => rrf.RequestedDate == updatedRRFModel.RRF.RequestedDate).OrderByDescending(a => a.ID).FirstOrDefault();


                var newRRFDetail = Mapper.Map<RRFDetailsViewModels, RRFDetail>(updatedRRFModel.RRFDetail);
                newRRFDetail.RRFId = newRRF.ID;
                newRRFDetail.ID = 0;
                isRRFCreated = service.Create<RRFDetail>(newRRFDetail, detail => detail.RRFId == newRRFDetail.RRFId);

                foreach (var skills in updatedRRFModel.RRFSkill)
                {
                    var newSkill = Mapper.Map<RRFSkillsViewModel, RRFSkill>(skills);
                    newSkill.ID = 0;
                    newSkill.RRFId = newRRF.ID;
                    isSkillCreated = service.Create<RRFSkill>(newSkill, s => s.ID == newSkill.ID);
                }

                var newComments = Mapper.Map<RRFCommentViewModel, RRFComments>(updatedRRFModel.RRFComments.FirstOrDefault());
                newComments.RRFId = newRRF.ID;
                isCommentUpdated = service.Create<RRFComments>(newComments, detail => detail.RRFId == newRRF.ID);

                if (isRRFCreated && isSkillCreated && isCommentUpdated)
                    service.Finalize(true);

                return GetRRFDetails(updatedRRFModel.RRF.ID);
            });
        }

        public async Task<RRFCandidateViewModel> AssignCandidate(RRFCandidateViewModel assignedCandidate)
        {
            return await Task.Run(() =>
            {
                var isCandidateAssigned = false;

                var assignNewCandidate = Mapper.Map<RRFCandidateViewModel, RRFCandidate>(assignedCandidate);
                isCandidateAssigned = service.Create<RRFCandidate>(assignNewCandidate, candidate => candidate.RRFId == assignNewCandidate.RRFId);

                if (isCandidateAssigned)
                    service.Finalize(true);

                return assignedCandidate;
            });
        }

        public async Task<int> BifercateRRF(int rrfId, string rrfNumber, int positions)
        {
            var selectedRRF = service.First<RRF>(x => x.ID == rrfId);
            for (int pos = 0; pos < positions; pos++)
            {
                var newRRFNumber = rrfNumber + "." + pos;
                var newRRF = await GetRRFDetails(rrfId);
                newRRF.RRF.ID = 0;
                newRRF.RRF.RRFNumber = newRRFNumber;
                var newRRFAdded = await AddRRF(newRRF);
            }
            return service.Finalize(true);
        }

        Task<IEnumerable<T>> IDataService.List<T>(string filters)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<T>> IDataService.Single<T>(string filters)
        {
            throw new NotImplementedException();
        }

        int IDataService.Add<T>(T model)
        {
            throw new NotImplementedException();
        }

        int IDataService.Update<T>(T model)
        {
            throw new NotImplementedException();
        }

        int IDataService.Delete<T>(int id)
        {
            throw new NotImplementedException();
        }

        public Task<RRFViewModel> GetRRFList(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
    }
}