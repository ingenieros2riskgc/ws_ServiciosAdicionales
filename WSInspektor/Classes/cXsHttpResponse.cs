using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ListasCliente.Classes
{
    public class cXsHttpResponse<T>
    {
        public bool HasError { get; set; }

        public string ErrorMessage { get; set; }

        public T Data { get; set; }
    }
}