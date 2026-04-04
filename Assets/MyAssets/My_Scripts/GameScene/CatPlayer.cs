
public class CatPlayer : CatSystem
{   public void MoveCatSet(GameSystem setSystem)
    {
        system = setSystem;
        targetObject = system.targetObject.transform;
        sphereObject = system.sphere.transform;
        AssignVariables();
    }
}
