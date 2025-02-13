using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ISaveToStageService
    {
        int SaveModelToStage<T>(int id, int recordID, ChangeSet<T> model);

        int SaveModelToMultiRecordStage<T>(int id, ChangeSet<T> model, bool isDeleted) where T : IBaseModel;
    }
}