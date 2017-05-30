﻿using Newtonsoft.Json;
using uIntra.Core.User;
using uIntra.Core.User.Permissions;
using uIntra.Users;
using Umbraco.Web.WebApi;
using Role = uIntra.Core.User.Role;

namespace Compent.uIntra.Controllers
{
    public class TestController : UmbracoApiController
    {
        private readonly IIntranetUserService<IntranetUser> _intranetUserService;
        private readonly IPermissionsConfiguration _permissionsConfiguration;
        private readonly IPermissionsService _permissionsService;

        public TestController(IIntranetUserService<IntranetUser> intranetUserService,
            IPermissionsConfiguration permissionsConfiguration,
            IPermissionsService permissionsService)
        {
            _intranetUserService = intranetUserService;
            _permissionsConfiguration = permissionsConfiguration;
            _permissionsService = permissionsService;
        }

        public string Get(string key)
        {
            var role = new Role
            {
                Name = IntranetRolesEnum.WebMaster.ToString(),
                Priority = IntranetRolesEnum.WebMaster.GetHashCode()
            };

            return JsonConvert.SerializeObject(_permissionsService.IsRoleHasPermissions(role, key));
        }
    }
}