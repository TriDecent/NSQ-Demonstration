﻿using Common.InputProvider;
using Common.Logger;
using Common.PayloadGenerator;
using Publisher.Scenarios;

// var demoScenario = new Scenario();
// await demoScenario.ExecuteAsync();

// var scenario1 = new Scenario1();
// await scenario1.ExecuteAsync();

// var scenario2 = new Scenario2();
// await scenario2.ExecuteAsync();

// var scenario3 = new Scenario3();
// await scenario3.ExecuteAsync();

var scenario4 = new Scenario4(new ConsoleInputProvider(), new ConsoleLogger(), new RandomPayloadGenerator());
await scenario4.ExecuteAsync();