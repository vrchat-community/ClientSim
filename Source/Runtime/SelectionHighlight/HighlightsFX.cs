using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class HighlightsFX : PostEffectsBase
{
    public static HighlightsFX Instance { get; protected set; }

    protected readonly HashSet<Renderer> objectsToRender = new HashSet<Renderer>();
    protected Shader highlightShader;
    protected Material highlightMaterial;

    #region HighlightsFX Singleton Methods

    public static void EnableObjectHighlight(Renderer outlineRenderer, bool enable)
    {
        if(Instance != null)
        {
            Instance.EnableOutline(outlineRenderer, enable);
        }
    }

    public void EnableOutline(Renderer outlineRenderer, bool enable)
    {
        if(outlineRenderer == null)
        {
            return;
        }

        if(enable)
        {
            objectsToRender.Add(outlineRenderer);
        }
        else
        {
            objectsToRender.Remove(outlineRenderer);
        }
    }

    #endregion
    
    #region Constants

    private const int DRAW_DEPTH_PREPASS = 0;
    private const int DRAW_HIGHLIGHT_PASS = 1;

    #endregion
    
    #region Unity Serialized Fields

    [SerializeField]
    private Color highlightColor = new Color(0.0f, 0.573f, 1.0f, 1.0f);

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float mobilePulseSpeed = 2.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float mobileMinimumOpacity = 0.2f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float mobileMaximumOpacity = 0.4f;

    #endregion

    #region Private Fields

    private static readonly int highlightColorId = Shader.PropertyToID("_HighlightColor");

    #endregion

    #region Unity Methods

    protected virtual void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("HighlightsFX - More than one instance detected.");
            return;
        }

        Instance = this;
        
        highlightShader = Shader.Find("Hidden/VRChat/MobileHighlight");
    }
    
    private void OnPostRender()
    {
        DrawMobileHighlight();
    }

    protected virtual void OnDestroy()
    {
        Instance = null;
    }

    #endregion
    
    #region Mobile Highlight

    private void DrawMobileHighlight()
    {
        objectsToRender.RemoveWhere(o => o == null);

        float highlightAlpha = Mathf.Lerp(mobileMinimumOpacity, mobileMaximumOpacity, 0.5f * Mathf.Sin(mobilePulseSpeed * Time.timeSinceLevelLoad) + 0.5f);
        highlightMaterial.SetColor(highlightColorId, new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * highlightAlpha));

        foreach(Renderer highlightedRenderer in objectsToRender)
        {
            MeshFilter meshFilter = highlightedRenderer.GetComponent<MeshFilter>();
            if(meshFilter == null)
            {
                continue;
            }

            if(meshFilter.sharedMesh == null)
            {
                continue;
            }

            Mesh sharedMesh = meshFilter.sharedMesh;
            Matrix4x4 localToWorldMatrix = highlightedRenderer.transform.localToWorldMatrix;
            highlightMaterial.SetPass(DRAW_DEPTH_PREPASS);
            Graphics.DrawMeshNow(sharedMesh, localToWorldMatrix);
        }

        foreach(Renderer highlightedRenderer in objectsToRender)
        {
            MeshFilter meshFilter = highlightedRenderer.GetComponent<MeshFilter>();
            if(meshFilter == null)
            {
                continue;
            }

            if(meshFilter.sharedMesh == null)
            {
                continue;
            }

            Mesh sharedMesh = meshFilter.sharedMesh;
            Matrix4x4 localToWorldMatrix = highlightedRenderer.transform.localToWorldMatrix;
            highlightMaterial.SetPass(DRAW_HIGHLIGHT_PASS);
            Graphics.DrawMeshNow(sharedMesh, localToWorldMatrix);
        }
    }

    #endregion


    #region PostEffectBase Methods

    public override bool CheckResources()
    {
        CheckSupport(false);
        highlightMaterial = CheckShaderAndCreateMaterial(highlightShader, highlightMaterial);

        if(!isSupported)
        {
            ReportAutoDisable();
        }

        return isSupported;
    }

    #endregion
}
