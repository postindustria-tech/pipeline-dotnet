using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FiftyOne.Pipeline.Math;
using FiftyOne.Pipeline.Web.Services;

namespace performance_tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessController : ControllerBase
    {
        private IFlowDataProvider _flow;

        public ProcessController(IFlowDataProvider flow)
        {
            _flow = flow;
        }

        [HttpGet]
        public string Get(){
            var math = _flow.GetFlowData()?.Get(MathElement.math);
            if(math != null) {
                return $"{math.Operation} = {math.Result}";
            }
            return "math engine data was null";
        }
    }
}