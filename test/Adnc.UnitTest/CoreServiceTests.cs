﻿using System;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using Autofac;
using Xunit.Abstractions;
using Adnc.UnitTest.Base;
using Adnc.UnitTest.Fixtures;
using Adnc.Core.Shared;
using Adnc.Cus.Core.Entities;
using Adnc.Core.Shared.IRepositories;
using Adnc.Infr.Common;
using Adnc.Infr.Common.Helper;
using Adnc.Cus.Core.CoreServices;

namespace Adnc.UnitTest
{
    public class CoreServiceTests : IClassFixture<CoreServiceFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly UserContext _userContext;
        private readonly ICusManagerService _cusManger;
        private readonly IEfRepository<Customer> _cusRsp;
        private CoreServiceFixture _fixture;


        public CoreServiceTests(CoreServiceFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _cusRsp = _fixture.Container.Resolve<IEfRepository<Customer>>();
            _cusManger = _fixture.Container.Resolve<ICusManagerService>();
            _userContext = _fixture.Container.Resolve<UserContext>();
            Initialize();
        }

        private void Initialize()
        {
            _userContext.ID = 1600000000000;
            _userContext.Account = "alpha2008";
            _userContext.Name = "余小猫";
        }

        /// <summary>
        /// 测试事务拦截器
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestUowInterceptor()
        {
            var id = IdGenerater.GetNextId(IdGenerater.DatacenterId, IdGenerater.WorkerId);
            var customer = new Customer() { ID = id, Account = "alpha2008", Nickname = IdGenerater.GetNextId().ToString(), Realname = IdGenerater.GetNextId().ToString() };
            var cusFinance = new CusFinance { Account = "alpha2008", ID = id, Balance = 0 };
            /*
                set session transaction isolation level repeatable read
                start transaction
                INSERT INTO `Customer` (`ID`, `Account`, `CreateBy`, `CreateTime`, `ModifyBy`, `ModifyTime`, `Nickname`, `Realname`)
                VALUES (122339207606833152, 'alpha2008', 1600000000000, timestamp('2020-12-03 14:12:20.552579'), NULL, NULL, '1606975940001', '1606975940002')
                INSERT INTO `CusFinance` (`ID`, `Account`, `Balance`, `CreateBy`, `CreateTime`, `ModifyBy`, `ModifyTime`)
                VALUES (122339207606833152, 'alpha2008', 0, 1600000000000, timestamp('2020-12-03 14:12:20.756977'), NULL, NULL)
                commit
             */ 
            await _cusManger.Register(customer, cusFinance);

            bool exists = await _cusRsp.ExistAsync(c => c.ID == id);
            Assert.True(exists);
        }
    }
}
