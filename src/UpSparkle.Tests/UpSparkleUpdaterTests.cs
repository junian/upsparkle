using System.Reflection;
using UpSparkle.Tests.Fakes;
using UpSparkle.Tests.Helpers;

namespace UpSparkle.Tests;

[TestClass]
public sealed class UpSparkleUpdaterTests
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AppcastUrlMetadataKey_HasExpectedValue()
    {
        Assert.AreEqual("SUFeedURL", UpSparkle.UpSparkleSettings.SUFeedURL);
    }

    [TestMethod]
    public void EdDSAPublicKeyMetadataKey_HasExpectedValue()
    {
        Assert.AreEqual("SUPublicEDKey", UpSparkle.UpSparkleSettings.SUPublicEDKey);
    }

    // -------------------------------------------------------------------------
    // Initial state (before Initialize is called)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsInitialized_IsFalse_BeforeInitialize()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        Assert.IsFalse(updater.IsInitialized);
    }

    [TestMethod]
    public void Properties_AreNull_BeforeInitialize()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        Assert.IsNull(updater.AppcastUrl);
        Assert.IsNull(updater.EdDSAPublicKey);
        Assert.IsNull(updater.CompanyName);
        Assert.IsNull(updater.AppName);
        Assert.IsNull(updater.AppVersion);
    }

    // -------------------------------------------------------------------------
    // Initialize(Assembly, …) — happy path
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Initialize_Assembly_SetsIsInitializedToTrue()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(TestAssemblyBuilder.Build());
        Assert.IsTrue(updater.IsInitialized);
    }

    [TestMethod]
    public void Initialize_Assembly_SetsPropertiesFromAssemblyAttributes()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(TestAssemblyBuilder.Build(
            companyName: "Acme",
            appName: "RocketApp",
            appVersion: "2.3.4",
            appcastUrl: "https://acme.com/appcast.xml",
            edDSAPublicKey: "abc123=="));

        Assert.AreEqual("Acme", updater.CompanyName);
        Assert.AreEqual("RocketApp", updater.AppName);
        Assert.AreEqual("2.3.4", updater.AppVersion);
        Assert.AreEqual("https://acme.com/appcast.xml", updater.AppcastUrl);
        Assert.AreEqual("abc123==", updater.EdDSAPublicKey);
    }

    [TestMethod]
    public void Initialize_Assembly_ExplicitAppcastUrlOverridesMetadata()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(
            TestAssemblyBuilder.Build(appcastUrl: "https://metadata.example.com/appcast.xml"),
            appcastUrl: "https://explicit.example.com/appcast.xml");

        Assert.AreEqual("https://explicit.example.com/appcast.xml", updater.AppcastUrl);
    }

    [TestMethod]
    public void Initialize_Assembly_ExplicitEdDSAKeyOverridesMetadata()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(
            TestAssemblyBuilder.Build(edDSAPublicKey: "metadataKey=="),
            edDSAPublicKey: "explicitKey==");

        Assert.AreEqual("explicitKey==", updater.EdDSAPublicKey);
    }

    [TestMethod]
    public void Initialize_Assembly_FallsBackToMetadataWhenParamsAreNull()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(
            TestAssemblyBuilder.Build(
                appcastUrl: "https://fallback.example.com/appcast.xml",
                edDSAPublicKey: "fallbackKey=="),
            appcastUrl: null,
            edDSAPublicKey: null);

        Assert.AreEqual("https://fallback.example.com/appcast.xml", updater.AppcastUrl);
        Assert.AreEqual("fallbackKey==", updater.EdDSAPublicKey);
    }

    [TestMethod]
    public void Initialize_Assembly_FallsBackToMetadataWhenParamsAreWhitespace()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(
            TestAssemblyBuilder.Build(
                appcastUrl: "https://fallback.example.com/appcast.xml",
                edDSAPublicKey: "fallbackKey=="),
            appcastUrl: "   ",
            edDSAPublicKey: "   ");

        Assert.AreEqual("https://fallback.example.com/appcast.xml", updater.AppcastUrl);
        Assert.AreEqual("fallbackKey==", updater.EdDSAPublicKey);
    }

    [TestMethod]
    public void Initialize_Assembly_StripsBuildMetadataSuffixFromVersion()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        updater.Initialize(TestAssemblyBuilder.Build(appVersion: "3.0.1+abc123"));

        Assert.AreEqual("3.0.1", updater.AppVersion);
    }

    [TestMethod]
    public void Initialize_Assembly_SkipsEdDSAKey_WhenNotProvided()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build(edDSAPublicKey: null), edDSAPublicKey: null);

        Assert.AreEqual(0, fake.SetEdDSAPublicKeyCallCount);
        Assert.IsNull(updater.EdDSAPublicKey);
    }

    [TestMethod]
    public void Initialize_Assembly_CallsNativeMethodsInOrder()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        Assert.AreEqual(1, fake.SetAppDetailsCallCount);
        Assert.AreEqual(1, fake.SetAppcastUrlCallCount);
        Assert.AreEqual(1, fake.InitializeCallCount);
    }

    [TestMethod]
    public void Initialize_Assembly_ForwardsAppDetailsToNative()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build(
            companyName: "Corp",
            appName: "MyApp",
            appVersion: "9.0.0"));

        Assert.AreEqual("Corp", fake.LastCompanyName);
        Assert.AreEqual("MyApp", fake.LastAppName);
        Assert.AreEqual("9.0.0", fake.LastAppVersion);
    }

    [TestMethod]
    public void Initialize_Assembly_ForwardsAppcastUrlToNative()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build(appcastUrl: "https://corp.com/feed.xml"));

        Assert.AreEqual("https://corp.com/feed.xml", fake.LastAppcastUrl);
    }

    [TestMethod]
    public void Initialize_Assembly_ForwardsEdDSAKeyToNative()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build(edDSAPublicKey: "myKey=="));

        Assert.AreEqual("myKey==", fake.LastEdDSAPublicKey);
    }

    // -------------------------------------------------------------------------
    // Initialize(Assembly, …) — error paths
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Initialize_Assembly_ThrowsArgumentNullException_WhenAssemblyIsNull()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        Assert.ThrowsExactly<ArgumentNullException>(
            () => updater.Initialize(null!));
    }

    [TestMethod]
    public void Initialize_Assembly_ThrowsArgumentException_WhenCompanyNameMissing()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        var assembly = TestAssemblyBuilder.Build(companyName: null);

        Assert.ThrowsExactly<ArgumentException>(
            () => updater.Initialize(assembly));
    }

    // -------------------------------------------------------------------------
    // InitializeAsync(Assembly, …)
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task InitializeAsync_Assembly_SetsIsInitializedToTrue()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        await updater.InitializeAsync(TestAssemblyBuilder.Build());
        Assert.IsTrue(updater.IsInitialized);
    }

    [TestMethod]
    public async Task InitializeAsync_Assembly_SetsPropertiesCorrectly()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        await updater.InitializeAsync(TestAssemblyBuilder.Build(
            companyName: "AsyncCorp",
            appName: "AsyncApp",
            appVersion: "4.5.6",
            appcastUrl: "https://async.example.com/appcast.xml",
            edDSAPublicKey: "asyncKey=="));

        Assert.AreEqual("AsyncCorp", updater.CompanyName);
        Assert.AreEqual("AsyncApp", updater.AppName);
        Assert.AreEqual("4.5.6", updater.AppVersion);
        Assert.AreEqual("https://async.example.com/appcast.xml", updater.AppcastUrl);
        Assert.AreEqual("asyncKey==", updater.EdDSAPublicKey);
    }

    [TestMethod]
    public async Task InitializeAsync_Assembly_ReturnsCompletedTask()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        var task = updater.InitializeAsync(TestAssemblyBuilder.Build());
        await task;
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task InitializeAsync_Assembly_ThrowsArgumentNullException_WhenAssemblyIsNull()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => updater.InitializeAsync(null!));
    }

    // -------------------------------------------------------------------------
    // CheckUpdateWithUI
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CheckUpdateWithUI_ThrowsInvalidOperationException_WhenNotInitialized()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        Assert.ThrowsExactly<InvalidOperationException>(
            () => updater.CheckUpdateWithUI());
    }

    [TestMethod]
    public void CheckUpdateWithUI_CallsNative_WhenInitialized()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        updater.CheckUpdateWithUI();

        Assert.AreEqual(1, fake.CheckUpdateWithUICallCount);
    }

    [TestMethod]
    public void CheckUpdateWithUI_CanBeCalledMultipleTimes()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        updater.CheckUpdateWithUI();
        updater.CheckUpdateWithUI();
        updater.CheckUpdateWithUI();

        Assert.AreEqual(3, fake.CheckUpdateWithUICallCount);
    }

    // -------------------------------------------------------------------------
    // CheckUpdateWithUIAsync
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task CheckUpdateWithUIAsync_ThrowsInvalidOperationException_WhenNotInitialized()
    {
        using var updater = new UpSparkleUpdater(new FakeNativeSparkle());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => updater.CheckUpdateWithUIAsync());
    }

    [TestMethod]
    public async Task CheckUpdateWithUIAsync_CallsNative_WhenInitialized()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        await updater.CheckUpdateWithUIAsync();

        Assert.AreEqual(1, fake.CheckUpdateWithUICallCount);
    }

    [TestMethod]
    public async Task CheckUpdateWithUIAsync_ReturnsCompletedTask()
    {
        var fake = new FakeNativeSparkle();
        using var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        var task = updater.CheckUpdateWithUIAsync();
        await task;

        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    // -------------------------------------------------------------------------
    // Automatic update configuration properties
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsAutomaticCheckForUpdates_DefaultsToFalse()
    {
        var fake = new FakeNativeSparkle();
        Assert.IsFalse(fake.IsAutomaticCheckForUpdates);
    }

    [TestMethod]
    public void IsAutomaticCheckForUpdates_CanBeSetAndRead()
    {
        var fake = new FakeNativeSparkle();

        fake.IsAutomaticCheckForUpdates = true;
        Assert.IsTrue(fake.IsAutomaticCheckForUpdates);

        fake.IsAutomaticCheckForUpdates = false;
        Assert.IsFalse(fake.IsAutomaticCheckForUpdates);
    }

    [TestMethod]
    public void UpdateCheckInterval_CanBeSetAndRead()
    {
        var fake = new FakeNativeSparkle();

        fake.UpdateCheckInterval = 86400;
        Assert.AreEqual(86400, fake.UpdateCheckInterval);

        fake.UpdateCheckInterval = 3600;
        Assert.AreEqual(3600, fake.UpdateCheckInterval);
    }

    [TestMethod]
    public void LastCheckTime_DefaultsToNull()
    {
        var fake = new FakeNativeSparkle();
        Assert.IsNull(fake.LastCheckTime);
    }

    [TestMethod]
    public void LastCheckTime_CanBeSetAndRead()
    {
        var fake = new FakeNativeSparkle();
        var lastCheckTime = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

        fake.LastCheckTime = lastCheckTime;
        Assert.AreEqual(lastCheckTime, fake.LastCheckTime);
    }

    // -------------------------------------------------------------------------
    // Dispose
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Dispose_CallsNativeDispose()
    {
        var fake = new FakeNativeSparkle();
        var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        updater.Dispose();

        Assert.AreEqual(1, fake.DisposeCallCount);
    }

    [TestMethod]
    public void Dispose_SetsIsInitializedToFalse()
    {
        var fake = new FakeNativeSparkle();
        var updater = new UpSparkleUpdater(fake);
        updater.Initialize(TestAssemblyBuilder.Build());

        updater.Dispose();

        Assert.IsFalse(updater.IsInitialized);
    }

    [TestMethod]
    public void Dispose_WorksBeforeInitialize()
    {
        var fake = new FakeNativeSparkle();
        var updater = new UpSparkleUpdater(fake);

        // Should not throw
        updater.Dispose();

        Assert.AreEqual(1, fake.DisposeCallCount);
        Assert.IsFalse(updater.IsInitialized);
    }
}
