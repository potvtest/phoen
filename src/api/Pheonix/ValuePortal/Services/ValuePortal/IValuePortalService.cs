//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Pheonix.Models.ViewModels;

//namespace ValuePortal.Services
//{
//    public interface IValuePortalService
//    {
//        Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaList(long userId, string Status, long loginuserId);
//        //Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListByUser(int userId);
//        Task<VCIdeaMasterViewModel> GetIdea(long id, long loginuserId);
//        Task<bool> SaveIdea(VCIdeaMasterViewModel idea);//,int userId);

//        Task<bool> UpdateIdea(VCIdeaMasterViewModel idea, int userId);
//        Task<bool> LockUnlockTheIdea(int ideaId, int userId);

//        Task<VPSubmittedCountViewModel> GetSubmittedIdeaCountbyId(long userId);
//        Task<bool> SaveComment(VPCommentsViewModel ideaVM);
//        //Task<IEnumerable<VPPriorityViewModel>> GetPriorityList();
//        // Task<IEnumerable<VPCommentsViewModel>> GetVPCommentData(long id);
//        Task<VPAllMastersdataViewModel> GetMastersdata();
//        Task<IEnumerable<VPConfigurationViewModel>> GetConfigurationdata();
//    }
//}
