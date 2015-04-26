# tesch [![Build Status](https://travis-ci.org/Vladislao/tesch.svg?branch=master)](https://travis-ci.org/Vladislao/tesch)
simple language to create tests for your C# code

## todo
* soon
  * complete with moq
  * complete with Nunit
* later
  * nspec
* paintitblack
  * visual studio integration
  * snippets
  * completiom
  * highlight
  
## examples
tesch version
```
use Sender PATH_TO_FILE

setup-fixture

setup

teardown

test SendContractToApi_ParsableXml_Sent
	mock IParserFactory setup ParseContract(any) returns new ContractModel
	act("string")
	mock IMongoRepository verify Publish(any) called once
```
Your default pain
```c#
using System;
using System.Collections.Generic;
using Autofac.Extras.Moq;
using Collector223.ApiConnector;
using Collector223.Core;
using Collector223.Core.Exceptions;
using Collector223.Core.Models;
using Collector223.Parser;
using EastpeasyMq;
using Moq;
using NUnit.Framework;
using UpdaterApi.Sender.Models;

namespace Collector223.ApiConnector.Tests
{
	[TestFixture]
	public class SenderTests
	{
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
		}

		[SetUp]
		public void SetUp()
		{
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void SendContractToApi_ParsableXml_Sent()
		{
			using (var mock = AutoMock.GetLoose())
			{
				mock.Mock<IParserFactory>().Setup(f => f.ParseContract(It.IsAny<object>())).Returns(() => new ContractModel());
				var actor = mock.Create<Sender>();
				actor.SendContractToApi("string");
				mock.Mock<IMongoRepository>().Verify(f => f.Publish(It.IsAny<object>()), Times.Once);
			}
		}
	}
}
```
