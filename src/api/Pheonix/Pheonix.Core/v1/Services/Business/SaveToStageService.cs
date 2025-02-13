using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using System;

namespace Pheonix.Core.v1.Services.Business
{
    public class SaveToStageService : ISaveToStageService
    {
        private IBasicOperationsService service;

        public SaveToStageService(IBasicOperationsService opsService)
        {
            service = opsService;
        }

        public int SaveModelToStage<T>(int id, int recordID, ChangeSet<T> model)
        {
            var oldStageEntry = service.First<Stage>(x => x.ModuleCode == model.ModuleCode && x.By == id);

            bool isSentForApproval = true;
            Stage newStageModel = new Stage
            {
                By = id,
                ModuleCode = model.ModuleCode,
                ModuleID = model.ModuleId,
                PreviousEntry = JsonConvert.SerializeObject(model.OldModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" }),
                NewEntry = JsonConvert.SerializeObject(model.NewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" }),
                RecordID = recordID,
                Date = DateTime.Now
            };
            if (oldStageEntry == null)
                isSentForApproval = service.Create<Stage>(newStageModel, x => x.ModuleCode == model.ModuleCode && x.By == id && x.ApprovalStatus == 0);
            else
                isSentForApproval = service.Update<Stage>(newStageModel, oldStageEntry);

            return service.Finalize(isSentForApproval);
        }

        public int SaveModelToMultiRecordStage<T>(int id, ChangeSet<T> model, bool isDeleted) where T : IBaseModel
        {
            var oldStageEntry = new MultiRecordStage();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                oldStageEntry = service.First<MultiRecordStage>(x => x.ModuleCode == model.ModuleCode && x.By == id && x.RecordID == model.OldModel.ID);
            }

            bool isSentForApproval = true;
            MultiRecordStage newStageModel = new MultiRecordStage
            {
                By = id,
                ModuleCode = model.ModuleCode,
                ModuleID = model.ModuleId,
                PreviousEntry = JsonConvert.SerializeObject(model.OldModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" }),
                NewEntry = JsonConvert.SerializeObject(model.NewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" }),
                RecordID = model.NewModel.ID,
                Date = DateTime.Now
            };

            if (isDeleted)
                newStageModel.StatusID = 3;
            else if (model.NewModel.ID == 0)
                newStageModel.StatusID = 1;
            else
                newStageModel.StatusID = 2;

            if (oldStageEntry == null || oldStageEntry.ID == 0)
            {
                if (!string.IsNullOrWhiteSpace(newStageModel.NewEntry))
                {
                    isSentForApproval = service.Create<MultiRecordStage>(newStageModel, x => x.ModuleCode == model.ModuleCode && x.By == id && x.RecordID == model.OldModel.ID && x.ApprovalStatus == 0);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(newStageModel.NewEntry))
                {
                    isSentForApproval = service.Update<MultiRecordStage>(newStageModel, oldStageEntry);
                }
            }

            return service.Finalize(isSentForApproval);
        }
    }
}