using AutoMapper;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminDeliveryTeamTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.deliveryTeam.Count > 0)
                {
                    foreach (DeliveryTeamModel value in model.deliveryTeam)
                    {
                        if (value.ID > 0)
                        {
                            var oldDeliveryTeam = repo.FindBy<DeliveryTeam>(x => x.ID == value.ID).FirstOrDefault();                          
                            oldDeliveryTeam.IsDeleted = value.IsDeleted;
                            oldDeliveryTeam.IsActive = value.IsActive;
                            if (value.IsActive == true)
                            {
                                oldDeliveryTeam.IsDeleted = false;
                            }

                            repo.Update<DeliveryTeam>(oldDeliveryTeam);
                            result.message = string.Format("Delivery Team updated successfully");
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            DeliveryTeam deliveryTeam = new DeliveryTeam();
                            deliveryTeam.Code = value.Code;
                            deliveryTeam.Name = value.Name;
                            deliveryTeam.IsDeleted = value.IsDeleted;
                            deliveryTeam.IsActive = value.IsActive;

                            repo.Create(deliveryTeam, null);
                            result.message = string.Format("Delivery Team added successfully");
                            result.isActionPerformed = true;

                        }
                        
                        
                        repo.Save();
                    }
                }
            }
            catch (Exception)
            {
                result.isActionPerformed = false;
                result.message = string.Format("operation Failed");
            }

            return result;
        }

        public AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminConfigActionModel configmodel = new AdminConfigActionModel();
            List<DeliveryTeam> list = repo.GetAll<DeliveryTeam>().ToList();
            configmodel.deliveryTeam = Mapper.Map<IEnumerable<DeliveryTeam>, List<DeliveryTeamModel>>(list);
            return configmodel;
        }
    }
}
