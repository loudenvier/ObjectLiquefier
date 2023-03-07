public class TestClassHierarchyEnumerator
{
    [Fact]
    public void GetClassHierarchyForTypeSystemObjectReturnsSingleSystemObjectType() {
        var hierarchy = new object().GetType().GetClassHierarchy();
        Assert.Equal(typeof(object), hierarchy.Single());
    }
    [Fact]
    public void GetClassHierarchyForObjectOfSystemObjectReturnsSingleSystemObjectType() {
        var hierarchy = new object().GetClassHierarchy();
        Assert.Equal(typeof(object), hierarchy.Single());
    }
    [Fact]
    public void GetClassHierarchyWorksForIEnumerables() {
        IEnumerable<int> ints = new int[] { 1, 2, 3 };
        var hierarchy = ints.GetClassHierarchy().ToArray();
        Assert.Equal(typeof(Int32[]), hierarchy[0]);
        Assert.Equal(typeof(Array), hierarchy[1]);
        Assert.Equal(typeof(object), hierarchy[2]);
    }
    [Fact]
    public void GetClassHierarchyWorkWithInterfaces() {
        var hierarchy = typeof(IThreadPoolWorkItem).GetClassHierarchy();
        Assert.Equal(typeof(IThreadPoolWorkItem), hierarchy.Single());
    }
    [Fact]
    public void GetClassHierarchyTreatsInheritedInterfaceAsImplementorOfBaseInterface() {
        var hierarchy = typeof(IInherited).GetClassHierarchy();
        Assert.Equal(typeof(IInherited), hierarchy.Single());
    }

    [Fact]
    public void GetClassHierarchyWorksForTypesInInterfacesReferences() {
        ITestInterface test = new TestInterface { Name = "Felipe" };
        var hierarchy = test.GetClassHierarchy().ToArray();
        Assert.Equal(typeof(TestInterface), hierarchy[0]);
    }

    public interface ITestInterface {
        public string Name { get; }
    }

    public interface IInherited : ITestInterface { }

    public class TestInterface : ITestInterface {
        public string Name { get; set; } = "";
    }

}
