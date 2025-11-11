using System.Reactive.Linq;
using AwesomeAssertions;
using KlabTranslations.Core;

namespace KlabTranslations.Core.Tests;

public sealed class TranslationParametersTests : IDisposable
{
    private readonly TranslationParameters _parameters;

    public TranslationParametersTests()
    {
        _parameters = new TranslationParameters();
    }

    public void Dispose()
    {
        _parameters.Dispose();
    }

    [Fact]
    public void IndexedParameter_SetAndGet_ShouldStoreAndRetrieveValue()
    {
        // Arrange
        const int index = 0;
        const string value = "test value";

        // Act
        _parameters[index] = value;
        object? result = _parameters[index];

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void IndexedParameter_GetNonExistent_ShouldReturnNull()
    {
        // Act
        object? result = _parameters[0];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IndexedParameter_SetMultipleIndices_ShouldStoreAllValues()
    {
        // Arrange
        const string value0 = "zero";
        const string value1 = "one";
        const string value2 = "two";

        // Act
        _parameters[0] = value0;
        _parameters[1] = value1;
        _parameters[2] = value2;

        // Assert
        _parameters[0].Should().Be(value0);
        _parameters[1].Should().Be(value1);
        _parameters[2].Should().Be(value2);
    }

    [Fact]
    public void IndexedParameter_SetNull_ShouldRemoveParameter()
    {
        // Arrange
        _parameters[0] = "value";

        // Act
        _parameters[0] = null;

        // Assert
        _parameters[0].Should().BeNull();
        _parameters.IndexedParameterCount.Should().Be(0);
    }

    [Fact]
    public void IndexedParameter_SetSameValue_ShouldNotNotifyChange()
    {
        // Arrange
        const string value = "same value";
        _parameters[0] = value;
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters[0] = value;

        // Assert
        changeCount.Should().Be(0);
        subscription.Dispose();
    }

    [Fact]
    public void IndexedParameter_UpdateValue_ShouldNotifyChange()
    {
        // Arrange
        _parameters[0] = "old value";
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters[0] = "new value";

        // Assert
        changeCount.Should().Be(1);
        subscription.Dispose();
    }

    [Fact]
    public void IndexedParameterCount_AfterSettingMultipleParameters_ShouldReflectCount()
    {
        // Act
        _parameters[0] = "zero";
        _parameters[1] = "one";
        _parameters[5] = "five";

        // Assert
        _parameters.IndexedParameterCount.Should().Be(3);
    }

    [Fact]
    public void IndexedParameterCount_AfterRemovingParameter_ShouldDecrement()
    {
        // Arrange
        _parameters[0] = "value";
        _parameters[1] = "value";

        // Act
        _parameters[0] = null;

        // Assert
        _parameters.IndexedParameterCount.Should().Be(1);
    }

    [Fact]
    public void NamedParameter_SetAndGet_ShouldStoreAndRetrieveValue()
    {
        // Arrange
        const string name = "username";
        const string value = "John";

        // Act
        _parameters[name] = value;
        object? result = _parameters[name];

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void NamedParameter_GetNonExistent_ShouldReturnNull()
    {
        // Act
        object? result = _parameters["nonexistent"];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NamedParameter_SetMultipleNames_ShouldStoreAllValues()
    {
        // Arrange
        const string name1 = "first";
        const string name2 = "second";
        const string name3 = "third";

        // Act
        _parameters[name1] = "value1";
        _parameters[name2] = "value2";
        _parameters[name3] = "value3";

        // Assert
        _parameters[name1].Should().Be("value1");
        _parameters[name2].Should().Be("value2");
        _parameters[name3].Should().Be("value3");
    }

    [Fact]
    public void NamedParameter_SetNull_ShouldRemoveParameter()
    {
        // Arrange
        const string name = "username";
        _parameters[name] = "John";

        // Act
        _parameters[name] = null;

        // Assert
        _parameters[name].Should().BeNull();
        _parameters.NamedParameterCount.Should().Be(0);
    }

    [Fact]
    public void NamedParameter_SetSameValue_ShouldNotNotifyChange()
    {
        // Arrange
        const string name = "username";
        const string value = "John";
        _parameters[name] = value;
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters[name] = value;

        // Assert
        changeCount.Should().Be(0);
        subscription.Dispose();
    }

    [Fact]
    public void NamedParameter_UpdateValue_ShouldNotifyChange()
    {
        // Arrange
        const string name = "username";
        _parameters[name] = "John";
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters[name] = "Jane";

        // Assert
        changeCount.Should().Be(1);
        subscription.Dispose();
    }

    [Fact]
    public void NamedParameterCount_AfterSettingMultipleParameters_ShouldReflectCount()
    {
        // Act
        _parameters["first"] = "value1";
        _parameters["second"] = "value2";
        _parameters["third"] = "value3";

        // Assert
        _parameters.NamedParameterCount.Should().Be(3);
    }

    [Fact]
    public void NamedParameterCount_AfterRemovingParameter_ShouldDecrement()
    {
        // Arrange
        _parameters["first"] = "value1";
        _parameters["second"] = "value2";

        // Act
        _parameters["first"] = null;

        // Assert
        _parameters.NamedParameterCount.Should().Be(1);
    }

    [Fact]
    public void MixedParameters_SetBothIndexedAndNamed_ShouldStoreAll()
    {
        // Act
        _parameters[0] = "indexed";
        _parameters["named"] = "value";

        // Assert
        _parameters[0].Should().Be("indexed");
        _parameters["named"].Should().Be("value");
        _parameters.IndexedParameterCount.Should().Be(1);
        _parameters.NamedParameterCount.Should().Be(1);
    }

    [Fact]
    public void HasParameters_WithNoParameters_ShouldReturnFalse()
    {
        // Act
        bool result = _parameters.HasParameters;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasParameters_WithIndexedParameters_ShouldReturnTrue()
    {
        // Arrange
        _parameters[0] = "value";

        // Act
        bool result = _parameters.HasParameters;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasParameters_WithNamedParameters_ShouldReturnTrue()
    {
        // Arrange
        _parameters["name"] = "value";

        // Act
        bool result = _parameters.HasParameters;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasParameters_WithBothIndexedAndNamed_ShouldReturnTrue()
    {
        // Arrange
        _parameters[0] = "indexed";
        _parameters["named"] = "value";

        // Act
        bool result = _parameters.HasParameters;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Clear_WithNoParameters_ShouldNotNotifyChange()
    {
        // Arrange
        int changeCount = 0;
        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters.Clear();

        // Assert
        changeCount.Should().Be(0);
        subscription.Dispose();
    }

    [Fact]
    public void Clear_WithParameters_ShouldRemoveAllAndNotify()
    {
        // Arrange
        _parameters[0] = "indexed";
        _parameters["named"] = "value";
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters.Clear();

        // Assert
        changeCount.Should().Be(1);
        _parameters.HasParameters.Should().BeFalse();
        _parameters[0].Should().BeNull();
        _parameters["named"].Should().BeNull();
        subscription.Dispose();
    }

    [Fact]
    public void Clear_WithIndexedParametersOnly_ShouldRemoveAllAndNotify()
    {
        // Arrange
        _parameters[0] = "zero";
        _parameters[1] = "one";
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters.Clear();

        // Assert
        changeCount.Should().Be(1);
        _parameters.IndexedParameterCount.Should().Be(0);
        subscription.Dispose();
    }

    [Fact]
    public void Clear_WithNamedParametersOnly_ShouldRemoveAllAndNotify()
    {
        // Arrange
        _parameters["first"] = "value1";
        _parameters["second"] = "value2";
        int changeCount = 0;

        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters.Clear();

        // Assert
        changeCount.Should().Be(1);
        _parameters.NamedParameterCount.Should().Be(0);
        subscription.Dispose();
    }

    [Fact]
    public void GetIndexedParametersArray_WithNoParameters_ShouldReturnEmptyArray()
    {
        // Act
        object?[] result = _parameters.GetIndexedParametersArray();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetIndexedParametersArray_WithConsecutiveIndices_ShouldReturnArrayInOrder()
    {
        // Arrange
        _parameters[0] = "zero";
        _parameters[1] = "one";
        _parameters[2] = "two";

        // Act
        object?[] result = _parameters.GetIndexedParametersArray();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("zero");
        result[1].Should().Be("one");
        result[2].Should().Be("two");
    }

    [Fact]
    public void GetIndexedParametersArray_WithGaps_ShouldIncludeNullValues()
    {
        // Arrange
        _parameters[0] = "zero";
        _parameters[2] = "two";
        _parameters[5] = "five";

        // Act
        object?[] result = _parameters.GetIndexedParametersArray();

        // Assert
        result.Should().HaveCount(6);
        result[0].Should().Be("zero");
        result[1].Should().BeNull();
        result[2].Should().Be("two");
        result[3].Should().BeNull();
        result[4].Should().BeNull();
        result[5].Should().Be("five");
    }

    [Fact]
    public void GetIndexedParametersArray_WithVariousTypes_ShouldPreserveTypes()
    {
        // Arrange
        _parameters[0] = "string";
        _parameters[1] = 42;
        _parameters[2] = 3.14;
        _parameters[3] = true;

        // Act
        object?[] result = _parameters.GetIndexedParametersArray();

        // Assert
        result.Should().HaveCount(4);
        result[0].Should().Be("string");
        result[1].Should().Be(42);
        result[2].Should().Be(3.14);
        result[3].Should().Be(true);
    }

    [Fact]
    public void GetIndexedParametersArray_IgnoresNamedParameters()
    {
        // Arrange
        _parameters[0] = "indexed";
        _parameters["named"] = "value";

        // Act
        object?[] result = _parameters.GetIndexedParametersArray();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be("indexed");
    }

    [Fact]
    public void GetNamedParametersDictionary_WithNoParameters_ShouldReturnEmptyDictionary()
    {
        // Act
        IReadOnlyDictionary<string, object?> result = _parameters.GetNamedParametersDictionary();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetNamedParametersDictionary_WithParameters_ShouldReturnAllNamedParameters()
    {
        // Arrange
        _parameters["first"] = "value1";
        _parameters["second"] = "value2";
        _parameters["third"] = "value3";

        // Act
        IReadOnlyDictionary<string, object?> result = _parameters.GetNamedParametersDictionary();

        // Assert
        result.Should().HaveCount(3);
        result["first"].Should().Be("value1");
        result["second"].Should().Be("value2");
        result["third"].Should().Be("value3");
    }

    [Fact]
    public void GetNamedParametersDictionary_WithVariousTypes_ShouldPreserveTypes()
    {
        // Arrange
        _parameters["string"] = "text";
        _parameters["number"] = 42;
        _parameters["decimal"] = 3.14;
        _parameters["boolean"] = false;

        // Act
        IReadOnlyDictionary<string, object?> result = _parameters.GetNamedParametersDictionary();

        // Assert
        result.Should().HaveCount(4);
        result["string"].Should().Be("text");
        result["number"].Should().Be(42);
        result["decimal"].Should().Be(3.14);
        result["boolean"].Should().Be(false);
    }

    [Fact]
    public void GetNamedParametersDictionary_IgnoresIndexedParameters()
    {
        // Arrange
        _parameters[0] = "indexed";
        _parameters["named"] = "value";

        // Act
        IReadOnlyDictionary<string, object?> result = _parameters.GetNamedParametersDictionary();

        // Assert
        result.Should().HaveCount(1);
        result["named"].Should().Be("value");
    }

    [Fact]
    public void GetNamedParametersDictionary_ReturnsReadOnlyDictionary()
    {
        // Arrange
        _parameters["key"] = "value";

        // Act
        IReadOnlyDictionary<string, object?> result = _parameters.GetNamedParametersDictionary();

        // Assert
        result.Should().BeAssignableTo<IReadOnlyDictionary<string, object?>>();
    }

    [Fact]
    public void IndexedParameter_WithObjectType_ShouldPreserveType()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        _parameters[0] = obj;
        object? result = _parameters[0];

        // Assert
        result.Should().Be(obj);
    }

    [Fact]
    public void NamedParameter_WithObjectType_ShouldPreserveType()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        _parameters["data"] = obj;
        object? result = _parameters["data"];

        // Assert
        result.Should().Be(obj);
    }

    [Fact]
    public void ParametersChanged_ShouldEmitEventOnIndexedParameterChange()
    {
        // Arrange
        bool eventFired = false;
        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => eventFired = true);

        // Act
        _parameters[0] = "value";

        // Assert
        eventFired.Should().BeTrue();
        subscription.Dispose();
    }

    [Fact]
    public void ParametersChanged_ShouldEmitEventOnNamedParameterChange()
    {
        // Arrange
        bool eventFired = false;
        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => eventFired = true);

        // Act
        _parameters["key"] = "value";

        // Assert
        eventFired.Should().BeTrue();
        subscription.Dispose();
    }

    [Fact]
    public void ParametersChanged_MultipleEvents_ShouldFireForEachChange()
    {
        // Arrange
        int changeCount = 0;
        IDisposable subscription = _parameters.ParametersChanged.Subscribe(_ => changeCount++);

        // Act
        _parameters[0] = "value1";
        _parameters[1] = "value2";
        _parameters["name"] = "value3";
        _parameters[0] = "changed";

        // Assert
        changeCount.Should().Be(4);
        subscription.Dispose();
    }
}
