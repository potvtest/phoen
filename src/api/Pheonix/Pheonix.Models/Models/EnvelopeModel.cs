using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.Models
{
    public class EnvelopeModel<T>
    {
         public EnvelopeModel(T t,bool isError,string errorMessage){
             this.Result = t;
             this.IsError = isError;
             this.ErrorMessage = errorMessage;
         }

        public T Result { get; set; }

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }

    }
}
