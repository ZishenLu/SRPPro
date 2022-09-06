using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEditor;

public class CustomShaderGenerator
{
    const string k_CustomShaderPath = "Assets/Scripts/Utils/CustomShader.Shader.txt";
    const string k_CustomHlslPath = "Assets/Scripts/Utils/CustomHlsl.hlsl.txt";

    [MenuItem("Assets/Create/Shader/Custom Shader")]
    public static void GenerateShader()
    {
        var targetObj = Selection.objects[0];
        string targetPath = AssetDatabase.GetAssetPath(targetObj);
        if(File.Exists(targetPath))
        {
            targetPath = Path.GetDirectoryName(targetPath); 
        }

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            ScriptableObject.CreateInstance<CreateScriptAsset>(),
            targetPath + "/CustomShader.Shader",
            null,
            k_CustomShaderPath
        );
        
    }
    [MenuItem("Assets/Create/Shader/Custom hlsl")]
    public static void GenerateHLSL()
    {
        var targetObj = Selection.objects[0];
        string targetPath = AssetDatabase.GetAssetPath(targetObj);

        if(File.Exists(targetPath))
        {
            targetPath = Path.GetDirectoryName(targetPath); 
        }

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            ScriptableObject.CreateInstance<CreateScriptAsset>(),
            targetPath + "/CustomHlsl.hlsl",
            null,
            k_CustomHlslPath
        );

    }
}
class CreateScriptAsset : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object obj = CreateTemplateScriptAsset(pathName,resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

    public UnityEngine.Object CreateTemplateScriptAsset(string newScriptPath,string targetPath)
    {
        string fullPath = Path.GetFullPath(newScriptPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(newScriptPath);
        string item = "#CustomShader#";
        if(targetPath.Contains("hlsl"))
        {
            item = "#HLSL#";
            fileNameWithoutExtension = fileNameWithoutExtension.ToUpper();
        }
        
        using (StreamWriter sw = new StreamWriter(fullPath,true,System.Text.Encoding.UTF8))
        {
            using (StreamReader sr = new StreamReader(targetPath))
            {
                string str = sr.ReadToEnd();
                str = Regex.Replace(str,item,fileNameWithoutExtension);
                sw.Write(str);
                sw.Close();
                sr.Close();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(newScriptPath);
                return AssetDatabase.LoadAssetAtPath(newScriptPath,typeof(UnityEngine.Object));
            }
            
        }
    }
}
