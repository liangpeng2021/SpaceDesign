/* Create by zh at 2019-06-25

   用于创建项目的标准文件夹路径 

 */

using UnityEditor;

public class CreateFolders : ScriptableWizard
{
    string rootPth = "Assets";
    public string userName = "ZH";
    [MenuItem("MyTools/CreateFolder")]
    static void CreateFolder()
    {
        ScriptableWizard.DisplayWizard(
            "CreateFolder", typeof(CreateFolders), "CreateAndClose");
    }

    /// <summary>
    /// Create按钮响应
    /// </summary>
    void OnWizardCreate()
    {
        if (AssetDatabase.IsValidFolder(rootPth + "/StreamingAssets") == false)
            AssetDatabase.CreateFolder(rootPth, "StreamingAssets");

        if (AssetDatabase.IsValidFolder(rootPth + "/Plugins") == false)
            AssetDatabase.CreateFolder(rootPth, "Plugins");

        string temp = rootPth + "/Temp";
        if (AssetDatabase.IsValidFolder(temp) == false)
            AssetDatabase.CreateFolder(rootPth, "Temp");

        if (AssetDatabase.IsValidFolder(temp + "/" + userName) == false)
            AssetDatabase.CreateFolder(temp, userName);

        string parentPth = rootPth + "/" + userName;

        if (AssetDatabase.IsValidFolder(parentPth) == false)
            AssetDatabase.CreateFolder(rootPth, userName);

        string[] folders = { "Animator", "Audios", "Fonts",
            "Materials", "Models", "MyPlugins", "Prefabs", "Resources",
            "Scripts", "Shaders", "Scenes", "Textures", };
        foreach(var v in folders)
        {
            if (AssetDatabase.IsValidFolder(parentPth + "/" + v) == false)
                AssetDatabase.CreateFolder(parentPth, v);
        }
    }
    /// <summary>
    /// Other按钮响应
    /// </summary>
    void OnWizardOtherButton() { }

    /// <summary>
    /// UI界面呼出响应
    /// </summary>
    void OnWizardUpdate() { }
}
