using System;
using System.Collections.Generic;
using System.Text;

namespace VAMedicalCallClient.ViewModels
{
    public interface IModuleViewModel
    {
        string ModuleTitle { get; }
        string ModuleSubTitle { get; }
    }
}
