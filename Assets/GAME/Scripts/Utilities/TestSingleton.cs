using UnityEngine;

public class TestSingleton : GenericSingleton<TestSingleton>
{
   public string testVal = "Hello, from Generic Singleton!";
}
