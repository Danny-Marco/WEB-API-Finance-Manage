using System.Collections.Generic;
using System.Linq;
using FinanceAccounting.Controllers;
using FinanceAccounting.Models;
using FinanceAccounting.Observer;
using FinanceAccounting.Observer.Interfaces;
using FinanceAccounting.Repositories.Interfaces;
using FinanceAccounting.Tests.TestRepositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FinanceAccounting.Tests
{
    public class AccountTests
    {
        private TestUnitOfWork _unitOfWork;
        private AccountsController _controller;
        private IActionResult _response;
        private Account _account;
        private IAccountRepository _accounts;
        private IOperationRepository _operations;

        [SetUp]
        public void Setup()
        {
            _operations = new TestOperationRepository();
            _accounts = new TestAccountRepository();
            _unitOfWork = new TestUnitOfWork(_accounts, _operations);
            _controller = new AccountsController(_unitOfWork);
        }

        [Test]
        public void GetAllAccounts_ShouldReturn_OkResult()
        {
            _response = _controller.GetAllAccounts();
            var expectedResult = _accounts.GetAll();

            var responseResult = (OkObjectResult) _response;
            var responseResultValue = responseResult.Value as List<Account>;

            var expectedCollectionJson = JsonConvert.SerializeObject(expectedResult);
            var actualCollectionJson = JsonConvert.SerializeObject(responseResultValue);

            Assert.That(_response, Is.TypeOf<OkObjectResult>());
            Assert.AreEqual(expectedCollectionJson, actualCollectionJson);
        }

        [Test]
        public void GetAccount_1_ShouldReturn_OkResult()
        {
            const int accountId = 1;
            var expectedAccount = _accounts.Get(accountId);

            _response = _controller.GetAccount(accountId);
            var responseResult = (OkObjectResult) _response;
            var responseResultValue = responseResult.Value as Account;

            var expectedAccountJson = JsonConvert.SerializeObject(expectedAccount);
            var actualAccountJson = JsonConvert.SerializeObject(responseResultValue);

            Assert.That(_response, Is.InstanceOf<OkObjectResult>());
            Assert.AreEqual(expectedAccountJson, actualAccountJson);
        }

        [Test]
        public void GetAccount_2_ShouldReturn_NotFoundResult()
        {
            var accountId = 404;

            _response = _controller.GetAccount(accountId);

            Assert.That(_response, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public void CreateAccount_Account_OkResult()
        {
            _account = new Account
            {
                AccountId = 200,
                CurrentSum = 333,
                Operations = new List<Operation>()
            };
            _response = _controller.CreateAccount(_account);

            var IsHasAccount = _unitOfWork.Accounts.GetAll().Any(a => a.AccountId == _account.AccountId);

            Assert.That(_response, Is.TypeOf<OkObjectResult>());
            Assert.That(IsHasAccount, Is.True);
        }

        [Test]
        public void CreateAccount_Account_BadRequest()
        {
            _account = null;
            var quantityAccount = _unitOfWork.Accounts.Count();

            _response = _controller.CreateAccount(_account);

            Assert.That(_response, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(_unitOfWork.Accounts.GetAll().Count(), Is.Not.EqualTo(quantityAccount + 1));
        }

        [Test]
        public void DeleteAccount_Account_BadRequest()
        {
            var accountId = 3;
            var accountsQuantity = _unitOfWork.Accounts.GetAll().Count;

            _response = _controller.DeleteAccount(accountId);

            Assert.That(_response, Is.TypeOf<NotFoundObjectResult>());
            Assert.That(_unitOfWork.Accounts.GetAll().Count, Is.Not.EqualTo(accountsQuantity - 1));
        }

        [Test]
        public void DeleteAccount_Account_OkResult()
        {
            var accountId = 2;
            var accountsQuantity = _unitOfWork.Accounts.GetAll().Count;

            _response = _controller.DeleteAccount(accountId);

            Assert.That(_response, Is.TypeOf<OkObjectResult>());
            Assert.That(_unitOfWork.Accounts.GetAll().Count, Is.EqualTo(accountsQuantity - 1));
        }

        [Test]
        public void UpdateAccount_Account_OKResult()
        {
            var account = new Account
            {
                AccountId = 11,
                CurrentSum = 1000,
                Operations = new List<Operation>()
            };

            var foundAccount = _accounts.Get(account.AccountId);
            var response = _controller.UpdateAccount(account);

            Assert.That(response, Is.TypeOf<OkObjectResult>());
            Assert.AreEqual(foundAccount.CurrentSum, account.CurrentSum);
            Assert.AreNotEqual(foundAccount.Operations, account.Operations);
        }
    }
}