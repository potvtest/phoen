using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using System;

namespace Pheonix.Core.v1.Services.Admin
{
    public class AdminCompOffTask : IAdminTask
    {
        //Leave Transaction

        public AdminActionResult TakeActionOn(IContextRepository repo, AdminActionModel model)
        {
            var existingCompOff = repo.FirstOrDefault<CompOff>(t => t.Person.ID == model.EmployeeID && t.ForDate == model.From);
            AdminActionResult result = new AdminActionResult();

            if (model.Validated == 1)
            {
                //model.Subtype = 1 means 'leave'
                //model.Subtype = 2 means 'comp-off'
                //model.Subtype = 9 means 'Casual Leave'
                //model.Subtype = 11 means 'Sick Leave'
                if (Convert.ToInt16(model.SubType) == 1)
                {
                    result = DebitCreditLeave(repo, model);
                }
                else if (Convert.ToInt16(model.SubType) == 2)
                {
                    result = DebitCreditCompOff(repo, model);
                }
                else if (Convert.ToInt16(model.SubType) == 9)
                {
                    result = DebitCreditCasualLeave(repo, model);
                }
                else if (Convert.ToInt16(model.SubType) == 11)
                {
                    result = DebitCreditSickLeave(repo, model);
                }

            }

            return result;
        }

        private AdminActionResult DebitCreditLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        repo.Create<PersonLeaveCredit>(new PersonLeaveCredit()
                        {
                            CreditBalance = model.Quantity,
                            PersonID = model.EmployeeID,
                            CreditedBy = model.AdminID,
                            DateEffective = model.From,
                            Narration = model.Comments,
                            Year = DateTime.Now.Year
                        }, null);
                        repo.Save();

                        if (model.Quantity < 0)
                        {
                            // 12/12/2017 : While leave deduction PersonLeaveLedger table should not updated
                            //var ledger = repo.FirstOrDefault<PersonLeaveLedger>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year);
                            //ledger.LeaveUtilized += (0 - model.Quantity);
                            //repo.Update(ledger);
                            //repo.Save();
                        }

                        transaction.Commit();

                        result.isActionPerformed = true;

                        if (model.Quantity > 0)
                            result.message = string.Format("Privilege Leave Credited Successfully");
                        else
                            result.message = string.Format("Privilege Leave Debited Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }

            return result;
        }

        private AdminActionResult DebitCreditCompOff(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.Quantity < 0)
                        {
                            int i = 0 - model.Quantity;
                            for (int j = 1; j <= i; j++)
                            {
                                var firstCompOff = repo.FirstOrDefault<CompOff>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year && t.ExpiresOn >= model.From && t.IsApplied != true && t.Status == 1);

                                if (firstCompOff != null)
                                {
                                    firstCompOff.Status = 3;
                                    firstCompOff.IsApplied = true;
                                    firstCompOff.Narration = "Admin Leave Deduction " + model.Comments;
                                    repo.Update(firstCompOff);
                                    repo.Save();
                                }
                            }

                            result.isActionPerformed = true;
                            result.message = string.Format("Comp-off Deleted Successfully");
                        }
                        else
                        {
                            repo.Create<CompOff>(new CompOff()
                            {
                                PersonID = model.EmployeeID,
                                ForDate = model.From,
                                ExpiresOn = model.From.AddMonths(3),
                                Year = model.From.Year,
                                Status = 1,
                                IsApplied = false,
                                Narration = model.Comments,
                                OnDate = DateTime.Now,
                                ByUser = model.AdminID
                            }, null);
                            repo.Save();

                            result.isActionPerformed = true;
                            result.message = string.Format("Comp-Off Credited Successfully");
                        }

                        // Increase or Decrease compoff quantity in comp off table
                        var ledger = repo.FirstOrDefault<PersonLeaveLedger>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year);

                        ledger.CompOffs += model.Quantity;
                        repo.Update(ledger);
                        repo.Save();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }

            return result;
        }

        private AdminActionResult DebitCreditCasualLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        repo.Create<PersonCLCredit>(new PersonCLCredit()
                        {
                            CreditBalance = model.Quantity,
                            PersonID = model.EmployeeID,
                            CreditedBy = model.AdminID,
                            DateEffective = model.From,
                            Narration = model.Comments,
                            Year = DateTime.Now.Year
                        }, null);

                        repo.Save();
                        transaction.Commit();

                        result.isActionPerformed = true;

                        if (model.Quantity > 0)
                            result.message = string.Format("Casual Leave Credited Successfully");
                        else
                            result.message = string.Format("Casual Leave Debited Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }

            return result;
        }

        private AdminActionResult DebitCreditSickLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        repo.Create<PersonSLCredit>(new PersonSLCredit()
                        {
                            CreditBalance = model.Quantity,
                            PersonID = model.EmployeeID,
                            CreditedBy = model.AdminID,
                            DateEffective = model.From,
                            Narration = model.Comments,
                            Year = DateTime.Now.Year
                        }, null);

                        repo.Save();
                        transaction.Commit();

                        result.isActionPerformed = true;

                        if (model.Quantity > 0)
                            result.message = string.Format("Sick Leave Credited Successfully");
                        else
                            result.message = string.Format("Sick Leave Debited Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }
            return result;
        }

        public AdminActionResult Delete(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.LeaveType == 0)
                {
                    result = DeleteCompOff(repo, model);
                }
                else if (model.LeaveType == 1)
                {
                    result = DeleteLeave(repo, model);
                }
                else if (model.LeaveType == 9)//Casual Leave
                {
                    result = DeleteCasualLeave(repo, model);

                }
                else if (model.LeaveType == 11)//Sick Leave
                {
                    result = DeleteSickLeave(repo, model);
                }

            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult DeleteLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var leave = repo.FirstOrDefault<PersonLeaveCredit>(x => x.ID == model.ID);
            repo.HardRemove<PersonLeaveCredit>(leave, x => x.ID == model.ID);
            repo.Save();

            result.isActionPerformed = true;
            result.message = string.Format("Privilege Leave Deleted Successfully");

            return result;
        }

        private AdminActionResult DeleteCompOff(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var compoff = repo.FirstOrDefault<CompOff>(x => x.ID == model.ID);
            if (compoff.IsApplied == false)
            {
                repo.HardRemove<CompOff>(compoff, x => x.ID == model.ID);
                repo.Save();


                var ledger = repo.FirstOrDefault<PersonLeaveLedger>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year);
                ledger.CompOffs -= 1;
                repo.Update(ledger);
                repo.Save();

                result.isActionPerformed = true;
                result.message = string.Format("Comp-off Deleted Successfully");
            }
            else
            {
                result.isActionPerformed = false;
                result.message = string.Format("Consumed Comp-Off can not be deleted");
            }

            return result;
        }

        private AdminActionResult DeleteCasualLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var casualLeave = repo.FirstOrDefault<PersonCLCredit>(x => x.ID == model.ID);
            repo.HardRemove<PersonCLCredit>(casualLeave, x => x.ID == model.ID);
            repo.Save();

            result.isActionPerformed = true;
            result.message = string.Format("Casual Leave Deleted Successfully");

            return result;
        }

        private AdminActionResult DeleteSickLeave(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var sickLeave = repo.FirstOrDefault<PersonSLCredit>(x => x.ID == model.ID);
            repo.HardRemove<PersonSLCredit>(sickLeave, x => x.ID == model.ID);
            repo.Save();

            result.isActionPerformed = true;
            result.message = string.Format("Sick Leave Deleted Successfully");

            return result;
        }
    }
}
