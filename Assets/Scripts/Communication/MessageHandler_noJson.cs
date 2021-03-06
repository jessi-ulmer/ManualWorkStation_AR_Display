using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// The class MessageHandler contains methods for parsing the messages coming from the IoT adapter and the MES.
/// </summary>

public class MessageHandler_noJson : MonoBehaviour
{
    private GameObject client;
    private GameObject assemblies;
    private GameObject product_turns;
    private GameObject product_holder;
    private GameObject feedback_canvas;
    private GameObject task_finished;
    private GameObject final_assembly_green;
    private GameObject camera;

    public int current_knowledge_level;
    public string current_version;
    private string current_producttype;
    private GameObject current_assembly_GO;

    private List<GameObject> holder_versions = new List<GameObject>();
    private List<GameObject> product_versions = new List<GameObject>();
    private List<GameObject> assembly_items = new List<GameObject>();
    private List<GameObject> turn_versions = new List<GameObject>();
    private List<GameObject> turn_operations = new List<GameObject>();
    private List<GameObject> active_items = new List<GameObject>();
    private List<GameObject> disabled_items = new List<GameObject>();
    
    // Materials
    private Material assembly_info_material_1;
    private Material assembly_info_material_2;
    private Material finished_info_material;
    private Material error_info_material;
    private Material transparent_material;

    // UI
    private GameObject setup_test;
    private GameObject current_point_display;
    private GameObject current_action_display;
    private GameObject max_point_display;
    private int max_points;
    private List<GameObject> uncompleted_steps = new List<GameObject>();
    private GameObject object_presentation;
    private GameObject assembly_presentation;
    private GameObject annotation;

    // UI: Miniature assembly
    private GameObject total_assembly_miniature;
    private List<GameObject> optically_changed_parts = new List<GameObject>();

    // Properties of Assemblies
    private float sizing_factor_v3 = 1.5f;

    void Start()
    {
        assembly_info_material_1 = (Material)Resources.Load("Materials/InformationMaterial1", typeof(Material));
        assembly_info_material_2 = (Material)Resources.Load("Materials/InformationMaterial2", typeof(Material));
        finished_info_material = (Material)Resources.Load("Materials/Green", typeof(Material));
        error_info_material = (Material)Resources.Load("Materials/Red", typeof(Material));
        transparent_material = (Material)Resources.Load("Materials/Transparent", typeof(Material));

        client = GameObject.Find("Client");

        // Find GO
        setup_test = GameObject.Find("Setup_Test");
        assemblies = GameObject.Find("Assemblies");
        product_turns = GameObject.Find("ProductTurns");
        product_holder = GameObject.Find("ProductHolder");
        object_presentation = GameObject.Find("NextObjects");
        assembly_presentation = GameObject.Find("TotalAssembly");
        task_finished = GameObject.Find("TaskFinished");
        camera = GameObject.Find("MainCamera");

        // Find Elements of Feedback Canvas
        feedback_canvas = GameObject.Find("FeedbackCanvas");
        current_action_display = feedback_canvas.transform.Find("General/ActionInfo").gameObject;
        annotation = feedback_canvas.transform.Find("General/Annotation").gameObject;
        current_point_display = feedback_canvas.transform.Find("Gamification/PointDisplay/CurrentPoints").gameObject;
        max_point_display = feedback_canvas.transform.Find("Gamification/PointDisplay/MaxPoints").gameObject;
    }

    public void InitializeVersion(string version_name)
    {
        // Reset everything
        ResetWorkplace();
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ResetFeedbackElements();
        if(current_assembly_GO != null)  // Remove previous product display
        {
            Destroy(current_assembly_GO);
        }

        // Set colors of gamification elements
        feedback_canvas.GetComponent<UI_FeedbackHandler>().InitializeQualityRate(80.0f, 60.0f);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().InitializeTimeRate(80.0f, 60.0f);

        // Load new work information
        current_version = version_name;  // Get V3.1
        current_producttype = current_version.Split('.')[0];  // Get V3
        Debug.Log("Load product version " + current_version);
        Debug.Log("Load product type " + current_producttype);
        current_action_display.GetComponent<Text>().text = "Load version " + current_version;

        product_versions = assemblies.GetComponent<AssemblyOrganisation>().main_items_list;
        holder_versions = product_holder.GetComponent<AssemblyOrganisation>().main_items_list;
        turn_versions = product_turns.GetComponent<AssemblyOrganisation>().main_items_list;
        try  // Load product holder
        {
            foreach (GameObject holder in holder_versions)
            {
                if (holder.name == current_version)
                {
                    holder.SetActive(true);
                }
                else
                {
                    holder.SetActive(false);
                }
            }
        }
        catch
        {
            Debug.LogWarning("No product holder specified for version " + current_version);
        }

        try // Load product cad
        {
            foreach (GameObject product in product_versions)  // Copy current product to highlight and modify it for the assembly instructions
            {
                if (product.name == current_version)
                {
                    current_assembly_GO = Instantiate(
                        original: product,
                        position: product.transform.position,
                        rotation: product.transform.rotation,
                        parent: product.transform.parent);
                    current_assembly_GO.SetActive(true);

                    try  // Show miniature product
                    {
                        if(total_assembly_miniature != null)
                        {
                            Destroy(total_assembly_miniature);
                        }
                        total_assembly_miniature = Instantiate(current_assembly_GO, new Vector3(0, 0, 0), product.transform.rotation, assembly_presentation.transform);
                        foreach (Transform part in total_assembly_miniature.transform)
                        {
                            foreach(Transform sub_part in part)  // Remove animations in miniature view
                            {
                                if (sub_part.name.Contains("Animation"))
                                {
                                    Destroy(sub_part.gameObject);
                                }
                                if (sub_part.name.Contains("Text"))
                                {
                                    Destroy(sub_part.gameObject);
                                }
                            }
                        }
                        total_assembly_miniature.transform.localPosition = new Vector3(0, 0, 0);
                        total_assembly_miniature.transform.localScale = 0.5f * total_assembly_miniature.transform.localScale;
                    }
                    catch
                    {
                        Debug.LogWarning("Product version could not be displayed " + current_version);
                    }

                    assembly_items = current_assembly_GO.GetComponent<AssemblyOrganisation>().main_items_list;  // Find GO of assembly
                    foreach (GameObject item in assembly_items)
                    {
                        item.SetActive(false);  // Deactivate GO that they are not visible
                    }
                }
                product.SetActive(false);

            }
        }
        catch
        {
            Debug.LogWarning("Product assembly not found of version " + current_version);
        }

        try  // Load turn operations
        {
            foreach (GameObject ver in turn_versions)
            {
                if (ver.name == current_version)
                {
                    ver.SetActive(true);
                    turn_operations = ver.GetComponent<AssemblyOrganisation>().main_items_list;
                }
                else
                {
                    ver.SetActive(false);
                }
            }
        }
        catch
        {
            Debug.LogWarning("Product turn operations not found of version " + current_version);
        }

        // Acknowledge init and send user information to hardware control
        Debug.Log(setup_test.GetComponent<Admin_PropertySelection>().user_name);
        client.GetComponent<Connection_noJson>().SendInformation("init_username[" + setup_test.GetComponent<Admin_PropertySelection>().user_name + "]level[" + setup_test.GetComponent<Admin_PropertySelection>().user_level + "]");

    }

    public void InitializeSteps(int number_steps)
    {
        Debug.Log("InitializeSteps: " + number_steps.ToString());
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ResetNumberSteps();
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ShowNumberSteps(number_steps);
    }

    public void InitializePoints (int number_points)
    {
        max_points = number_points;
        Debug.Log("InitializePoints: " + max_points.ToString());
        max_point_display.GetComponent<Text>().text = max_points.ToString();
        ShowPoints(0);
    }

    public void NewInstructions ()
    {
        // Debug.Log("New work step -> reset support");
        ResetWorkplace();
    }

    public void InitializeCamera(float x, float y, float z, float ortho)
    {
        camera.GetComponent<CameraHandler>().ChangeCameraSettings(x, y, z, ortho);
    }

    public void ParsePerformanceMessage(int total_points, float quality_performance, float time_performance, int total_level, string node_finished, string level_up, string perfect_run, 
        string message_text, int message_color_r, int message_color_g, int message_color_b)
    {
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ShowPoints(total_points);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ShowQualityRate(quality_performance);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ShowTimeRate(time_performance);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ShowLevel(total_level);
        if (node_finished == "True")
        {
            Debug.Log("Step finished");
            feedback_canvas.GetComponent<UI_FeedbackHandler>().FinishStep();
        }
        if(level_up == "True")
        {
            feedback_canvas.GetComponent<UI_FeedbackHandler>().DisplayLevelup();
        }
        if(perfect_run == "True")
        {
            feedback_canvas.GetComponent<UI_FeedbackHandler>().DisplayPerfectRun();
        }
        if(message_text != "")
        {
            feedback_canvas.GetComponent<UI_FeedbackHandler>().DisplayPopup(message_text, message_color_r, message_color_g, message_color_b);
        }
    }

    public void PickObject(string item_name, string led_color, int knowledge_level, int default_time)
    {
        // Demo application for events
        if(item_name == "thank_you")
        {
            current_action_display.GetComponent<Text>().text = "Thank You!";
            return;
        }

        // Find and show prefab
        GameObject item_prefab = FindPrefab("Prefabs/Parts/" + current_producttype + "/" + item_name, item_name);
        if (item_prefab == null)
        {
            Debug.LogWarning("Prefab for " + item_name + " not found");
            return;
        }

        if (led_color == "green")  // Correct pick -> show information according to level
        {
            Debug.Log("Show pick instruction for " + item_name);
            if (knowledge_level < 4)  // Show 3D image
            {
                ShowPickPrefab(item_prefab, "Pick item");
            }
            else if (knowledge_level == 4)  // Do not show 3D image
            {
            }
            else
            {
                Debug.LogWarning("Unknown level " + knowledge_level);
            }
            feedback_canvas.GetComponent<UI_FeedbackHandler>().StartTimer(default_time);
        }
        else if (led_color == "red")  // Wrong pick -> show error information independently from level
        {
            Debug.Log("Show error pick instruction for " + item_name);
            feedback_canvas.GetComponent<UI_FeedbackHandler>().NotifyWrongAction();
            ResetWorkplace();
            GameObject item = ShowPickPrefab(item_prefab, "Wrong pick, remove object");
            item.GetComponent<ObjectInteractions>().ChangeMaterial(error_info_material);
        }        
    }

    public void PickTool(string tool_name, string led_color, int knowledge_level, int default_time)
    {
        // Find prefab
        GameObject tool_prefab = FindPrefab("Prefabs/Tools/" + tool_name, tool_name);
        if (tool_prefab == null)
        {
            Debug.LogWarning("Prefab for " + tool_name + " not found");
            return;
        }

        // Show pick instructions
        if (led_color == "green")  // Correct pick -> show information according to level
        {
            Debug.Log("Show pick instruction for " + tool_name);
            if (knowledge_level < 4)  // Show 3D image
            {
                ShowPickPrefab(tool_prefab, "Pick tool");
            }
            else if (knowledge_level == 4)  // Do not show 3D image
            {
            }
            else
            {
                Debug.LogWarning("Unknown level " + knowledge_level);
            }
            feedback_canvas.GetComponent<UI_FeedbackHandler>().StartTimer(default_time);
        }
        else if (led_color == "red")  // Wrong pick -> show error information independently from level
        {
            Debug.Log("Show error pick instruction for " + tool_name);
            feedback_canvas.GetComponent<UI_FeedbackHandler>().NotifyWrongAction();
            ResetWorkplace();
            GameObject tool = ShowPickPrefab(tool_prefab, "Wrong pick, return tool");
            tool.GetComponent<ObjectInteractions>().ChangeMaterial(error_info_material);
        }
    }

    public void ReturnTool(string tool_name, string led_color, int knowledge_level, int default_time)
    {
        // Find prefab
        GameObject tool_prefab = FindPrefab("Prefabs/Tools/" + tool_name, tool_name);
        if (tool_prefab == null)
        {
            Debug.LogWarning("Prefab for " + tool_name + " not found");
            return;
        }

        // Show pick instructions
        Debug.Log("Show return instruction for " + tool_name);
        if (knowledge_level < 4)  // Show 3D image
        {
            ShowPickPrefab(tool_prefab, "Return tool");
        }
        else if (knowledge_level == 4)  // Do not show 3D image
        {
        }
        else
        {
            Debug.LogWarning("Unknown level " + knowledge_level);
        }
        feedback_canvas.GetComponent<UI_FeedbackHandler>().StartTimer(default_time);
    }

    public void ShowAssemblyInfos(string item_name, int knowledge_level, int default_time, string text_annotation)
    {
        if (item_name == "thank_you")
        {
            current_action_display.GetComponent<Text>().text = "Thank You!";
            feedback_canvas.GetComponent<UI_FeedbackHandler>().DisplayThankYou();
            return;
        }

        Debug.Log("Show assembly instruction for " + item_name + " in level " + knowledge_level);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().StartTimer(default_time);

        if (knowledge_level == 1)
        {
            current_action_display.GetComponent<Text>().text = "Assemble";
            annotation.GetComponent<Text>().text = text_annotation;
            annotation.GetComponent<UI_BackgroundImage>().annotation_change = true;
            ShowAssemblyPosition(assembly_info_material_1, item_name, disable_afterwards: true, change_material: false);
            ShowPositionMiniature(item_name);
        }
        else if (knowledge_level == 2)
        {
            current_action_display.GetComponent<Text>().text = "Assemble";
            GameObject item_go = ShowAssemblyPosition(assembly_info_material_1, item_name, disable_afterwards: true, change_material: false);
            RemoveAssemblyHints(item_go);
            ShowPositionMiniature(item_name);
        }
        else if (knowledge_level == 3)
        {
            current_action_display.GetComponent<Text>().text = "Assemble";
            ShowPositionMiniature(item_name);
        }
        else if (knowledge_level == 4)
        {
        }
        else
        {
            Debug.LogWarning("Unknown level " + knowledge_level);
        }
    }

    public void ShowToolUsage(string action_name, int knowledge_level, int default_time, string text_annotation)
    {
        Debug.Log("Show tool usage instruction for " + action_name);
        feedback_canvas.GetComponent<UI_FeedbackHandler>().StartTimer(default_time);

        if (knowledge_level == 1)
        {
            current_action_display.GetComponent<Text>().text = "Assemble with tool";
            annotation.GetComponent<Text>().text = text_annotation;
            annotation.GetComponent<UI_BackgroundImage>().annotation_change = true;
            ShowAssemblyPosition(assembly_info_material_2, action_name, disable_afterwards: true, change_material: true);
            ShowPositionMiniature(action_name);
        }
        else if (knowledge_level == 2)
        {
            current_action_display.GetComponent<Text>().text = "Assemble with tool";
            GameObject action_go = ShowAssemblyPosition(assembly_info_material_2, action_name, disable_afterwards: true, change_material: true);
            RemoveAssemblyHints(action_go);
            ShowPositionMiniature(action_name);
        }
        else if (knowledge_level == 3)
        {
            current_action_display.GetComponent<Text>().text = "Assemble with tool";
            ShowPositionMiniature(action_name);
        }
        else if (knowledge_level == 4)
        {

        }
        else
        {
            Debug.LogWarning("Unknown level " + knowledge_level);
        }        
    }

    private GameObject ShowAssemblyPosition(Material material, string item_name, bool disable_afterwards, bool change_material)
    {
        foreach (GameObject item in assembly_items)
        {
            if (item.name == item_name)
            {
                item.SetActive(true);
                active_items.Add(item);
                ShowObjectPosition(item, material, disable_afterwards, change_material);
                return item;
            }
        }
        return null;
    }

    private void RemoveAssemblyHints(GameObject current_item)
    {
        foreach (Transform part in current_item.transform)
        {
            if (part.name.Contains("Animation"))
            {
                part.gameObject.SetActive(false);
                disabled_items.Add(part.gameObject);
            }
            if (part.name.Contains("Text"))
            {
                part.gameObject.SetActive(false);
                disabled_items.Add(part.gameObject);
            }
        }
    }

    private void ShowPositionMiniature(string item_name)
    {
        total_assembly_miniature.SetActive(true);
        Debug.Log("Show part in miniature: " + item_name);
        GameObject current_mini_part = total_assembly_miniature.transform.Find(item_name).gameObject;
        ShowObjectPosition(current_mini_part, assembly_info_material_1, disable_afterwards: false, change_material: true);
    }

    public void ShowPoints(int current_points)
    {
        current_point_display.GetComponent<Text>().text = current_points.ToString();
        float ratio = current_points / max_points;

        if (ratio > 0.8f)
        {
            current_point_display.GetComponent<Text>().color = Color.green;
        }
        else if (ratio > 0.4f)
        {
            current_point_display.GetComponent<Text>().color = Color.cyan;
        }
        else
        {
            current_point_display.GetComponent<Text>().color = Color.yellow;
        }
    }

    public void FinishStep()
    {
        uncompleted_steps[0].GetComponent<Image>().color = Color.green;
        uncompleted_steps.RemoveAt(0);
    }

    public void FinishJob()
    {
        current_assembly_GO.GetComponent<ObjectInteractions>().ActivateAllChildren();

        final_assembly_green = Instantiate(
            original: current_assembly_GO,
            position: current_assembly_GO.transform.position,
            rotation: current_assembly_GO.transform.rotation,
            parent: current_assembly_GO.transform.parent);
        Destroy(current_assembly_GO);

        foreach (Transform part in final_assembly_green.transform)
        {
            foreach (Transform sub_part in part)  // Remove additional information
            {
                if (sub_part.name.Contains("Animation"))
                {
                    Destroy(sub_part.gameObject);
                }
                if (sub_part.name.Contains("Text"))
                {
                    Destroy(sub_part.gameObject);
                }
                if (sub_part.name.Contains("Toolpoint"))
                {
                    Destroy(sub_part.gameObject);
                }
            }
        }

        final_assembly_green.GetComponent<ObjectInteractions>().ChangeMaterial(finished_info_material);
        task_finished.GetComponent<AudioSource>().Play();
        current_action_display.GetComponent<Text>().text = "Task finished; remove assembly";

        // client.GetComponent<Connection_noJson>().SendInformation("{finished}");
    }

    private void ShowObjectPosition(GameObject current_object, Material material, bool disable_afterwards, bool change_material)
    {
        if(change_material)
        {
            if (current_object.GetComponent<ObjectInteractions>() == null)
            {
                current_object.AddComponent<ObjectInteractions>();
            }
            current_object.GetComponent<ObjectInteractions>().ChangeMaterial(material);
            optically_changed_parts.Add(current_object);
        }
        if (disable_afterwards)
        {
            active_items.Add(current_object);
        }

    }

    public void ResetWorkplace()
    {
        if (object_presentation.transform.childCount > 0)
        {
            foreach (Transform child in object_presentation.transform)
            {
                Destroy(child.gameObject);
            }
            object_presentation.transform.DetachChildren();  // Remove children from parent, otherwise childCount is not working in same frame
        }
        if (active_items.Count() > 0)
        {
            foreach (GameObject item in active_items)
            {
                item.SetActive(false);
            }
        }
        if(optically_changed_parts.Count() > 0)
        {
            foreach (GameObject part in optically_changed_parts.ToList())
            {
                part.GetComponent<ObjectInteractions>().ResetMaterial();
                optically_changed_parts.Remove(part);
            }
        }
        if(total_assembly_miniature != null)
        {
            total_assembly_miniature.SetActive(false);
        }
        if(final_assembly_green != null)
        {
            Destroy(final_assembly_green);
        }
        active_items.Clear();
        feedback_canvas.GetComponent<UI_FeedbackHandler>().ResetNotifications();
        annotation.GetComponent<Text>().text = "";
        annotation.GetComponent<UI_BackgroundImage>().annotation_change = true;
        current_action_display.GetComponent<Text>().text = "";
    }

    public GameObject FindGameobject(string name, List<GameObject> gameobject_list)
    {
        foreach (GameObject obj in gameobject_list)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }
        Debug.LogWarning("Gameobject " + name + " not found");
        return null;
    }

    private GameObject ShowPickPrefab(GameObject prefab, string display_text)
    {
        // Instantiate prefab and set parent
        GameObject displayed_item = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        Vector3 original_scale = displayed_item.transform.localScale;
        displayed_item.transform.parent = object_presentation.transform;
        displayed_item.transform.localRotation = prefab.transform.rotation;
        displayed_item.transform.localScale = original_scale * sizing_factor_v3;

        // Check if several pick options exist
        int number_pick_options = object_presentation.transform.childCount;
        Vector3 offset = new Vector3(0.5f, 0, 0);

        if (number_pick_options == 1)  // Show prefab at first pick position
        {
            displayed_item.transform.localPosition = new Vector3(0, 0, 0);

            current_action_display.GetComponent<Text>().text = display_text;
        }
        else  // Show prefab at subsequent pick position
        {
            Debug.Log("Number of pick options: " + number_pick_options.ToString());
            int movement = number_pick_options - 1;
            displayed_item.transform.localPosition = new Vector3(0, 0, 0) + movement * offset;

            current_action_display.GetComponent<Text>().text = "Pick objects";
        }

        // Add properties
        if (displayed_item.GetComponent<ObjectInteractions>() == null)
        {
            displayed_item.AddComponent<ObjectInteractions>();
        }

        return displayed_item;
    }

    private GameObject FindPrefab(string path_name, string prefab_name)
    {
        GameObject prefab = (GameObject)Resources.Load(path_name, typeof(GameObject));
        if (prefab == null)
        {
            Debug.LogWarning("Prefab not found:" + prefab_name);
        }
        return prefab;
    }
}
