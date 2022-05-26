using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

using CAMOWA.FileImporting;
using CAMOWA;
namespace Navinha
{
    [BepInDependency("locochoco.plugins.CAMOWA",BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("locochoco.plugin.nave","Navinha","1.0.0.0")]
    [BepInProcess("OuterWilds_Alpha_1_2.exe")]
    public class Navo : BaseUnityPlugin
    {
        private const string naveMeshFile = "navinhoca.obj";
        private const string naveTextureFile = "textura.jpg";
        private const string naveThrusterAudioFile = "naveThrusterAudio.wav";
        private static Mesh naveMesh;
        private static Material naveMaterial;
        private static AudioClip naveThrusterAudio;

        private GameObject nave;

        private static string gamePath;
        public static string DllExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(gamePath))
                    gamePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return gamePath;
            }

            private set { }
        }

        public void Awake()
        {
            SceneLoading.OnSceneLoad += SceneLoading_OnSceneLoad;
        }
        
        private void SceneLoading_OnSceneLoad(int sceneId)
        {
            if (sceneId == 1)
                AddNave();
        }

        public void Update()
        {
            if (Application.loadedLevel == 1)
            {
                if (nave == null)
                {
                    nave = CreateNave();
                    nave.SetActive(false);
                }
                if (Input.GetKeyUp(KeyCode.C))
                {
                    if (!nave.activeSelf)
                        nave.SetActive(true);
                    nave.transform.parent = Locator.GetSunTransform().root;
                    nave.transform.position = Camera.main.transform.forward * 3f + Camera.main.transform.position;
                    nave.transform.rotation = Locator.GetPlayerTransform().rotation;
                    nave.GetAttachedOWRigidbody().SetVelocity(Locator.GetPlayerTransform().GetAttachedOWRigidbody().GetVelocity());
                }
            }
        }
        public static void AddNave()
        {
            if (naveMesh == null)
            {
                naveMesh = FileImporter.ImportOBJMesh(Path.Combine(DllExecutablePath, naveMeshFile));
            }
            if (naveMaterial == null)
            {
                var shade = Shader.Find("Diffuse");
                naveMaterial = new Material(shade)
                {
                    mainTexture = FileImporter.ImportImage(Path.Combine(DllExecutablePath, naveTextureFile))
                };
            }
            if (naveThrusterAudio == null)
            {
                try
                {
                    naveThrusterAudio = FileImporter.ImportWAVAudio(Path.Combine(DllExecutablePath, naveThrusterAudioFile));
                }
                catch { Debug.Log("Erro ao ler o audio"); }
            }
            NaveInputs.InnitNaveInputs();
        }
        
        private GameObject CreateNave()
        {
            //Parte dos propulsores 
            GameObject naveBody = new GameObject("nave_body");
            naveBody.AddComponent<Rigidbody>().mass = 0.6f;
            OWRigidbody naveBodyRigid = naveBody.AddComponent<NaveBody>();
            naveBody.AddComponent<NaveThrusterModel>();
            NaveThrusterController naveThrusterController = naveBody.AddComponent<NaveThrusterController>();

            naveBody.AddComponent<NaveNoiseMaker>();

            LODLayer lodLayer =  naveBody.AddComponent<LODLayer>();
            HarmonyLib.AccessTools.FieldRefAccess<LODLayer, bool>(lodLayer, "_ignoreLOD") = true;
            HarmonyLib.AccessTools.FieldRefAccess<LODLayer, int>(lodLayer, "_lodLayer") = 0;
            //Assento
            GameObject naveSeat = new GameObject("nave_seat");
            naveSeat.transform.parent = naveBody.transform;

            CapsuleCollider assentoCollider= naveSeat.AddComponent<CapsuleCollider>();
            assentoCollider.radius = 0.5f;
            assentoCollider.height = 2f;

            naveSeat.AddComponent<InteractZone>().Init("Sentar");

            PlayerAttachPoint attachPoint = naveSeat.AddComponent<PlayerAttachPoint>();
            HarmonyLib.AccessTools.FieldRefAccess<PlayerAttachPoint, bool>(attachPoint, "_lockPlayerTurning") = true;
            HarmonyLib.AccessTools.FieldRefAccess<PlayerAttachPoint, bool>(attachPoint, "_centerCamera") = true;
            NaveFlightConsole naveFlightConsole = naveSeat.AddComponent<NaveFlightConsole>();
            naveFlightConsole.naveBody = naveBodyRigid;
            naveFlightConsole.naveThrusterController = naveThrusterController;


            naveSeat.transform.localPosition = new Vector3(0f, 1.1f, 0.5f);
            
            //Detector
            GameObject naveDetector = new GameObject("nave_detector");
            naveDetector.transform.parent = naveBody.transform;
            naveDetector.transform.localPosition = Vector3.zero;

            naveDetector.AddComponent<SphereCollider>();


            naveDetector.AddComponent<AlignmentFieldDetector>();
            naveDetector.AddComponent<SimpleFluidDetector>().SetDragFactor(2f);

            //Collider
            GameObject naveCollider = new GameObject("nave_collider");
            naveCollider.transform.parent = naveBody.transform;
            naveCollider.transform.localPosition = Vector3.zero;

            GameObject floorCollider = new GameObject("nave_collider_floor");
            floorCollider.transform.parent = naveCollider.transform;
            floorCollider.transform.localPosition = new Vector3(0.144f,0.149f,0f);
            floorCollider.AddComponent<BoxCollider>().size = new Vector3(1.5f, 0.3f, 0.62f);

            GameObject tankCollider = new GameObject("nave_collider_tank");
            tankCollider.transform.parent = naveCollider.transform;
            tankCollider.transform.localPosition = new Vector3(-0.63f, 0.67f, 0f);
            tankCollider.transform.localRotation = Quaternion.Euler(-90f,0f,0f);
            tankCollider.AddComponent<CapsuleCollider>();

            GameObject thrusterCollider = new GameObject("nave_collider_thruster");
            thrusterCollider.transform.parent = naveCollider.transform;
            thrusterCollider.transform.localPosition = new Vector3(-1.403f, 0.68f, 0f);
            thrusterCollider.transform.localRotation = Quaternion.Euler(0f, 0f, 98.65f);
            thrusterCollider.AddComponent<CapsuleCollider>().height = 1.72f;

            //Proxy collider
            GameObject naveProxyCollider = new GameObject("nave_proxyCollider");
            naveProxyCollider.layer = LayerMask.NameToLayer("ProxyPrimitive");

            naveProxyCollider.transform.parent = naveBody.transform;
            naveProxyCollider.transform.localPosition = Vector3.zero;

            GameObject hullCollider = new GameObject("nave_proxyCollider_hull");
            hullCollider.layer = LayerMask.NameToLayer("ProxyPrimitive");
            hullCollider.transform.parent = naveProxyCollider.transform;
            hullCollider.transform.localPosition = new Vector3(-0.48f, 1.18f, 0f);
            hullCollider.AddComponent<BoxCollider>().size = new Vector3(3f, 2.36f, 1.65f);


            naveProxyCollider.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            naveCollider.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            
            //Mesh
            GameObject naveMeshGO = new GameObject("nave_mesh");
            naveMeshGO.transform.parent = naveBody.transform;
            naveMeshGO.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            naveMeshGO.AddComponent<MeshFilter>().mesh = naveMesh;
            naveMeshGO.AddComponent<MeshRenderer>().sharedMaterial = naveMaterial;

            naveMeshGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            naveMeshGO.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            //Audio
            GameObject naveThrusterAudioSource = new GameObject("nave_thrusterAudioSource");
            naveThrusterAudioSource.transform.parent = naveBody.transform;
            naveThrusterAudioSource.transform.localPosition = new Vector3(0f, 0.68f, -1.403f);

            naveThrusterAudioSource.AddComponent<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
            OWAudioSource audioSource = naveThrusterAudioSource.AddComponent<OWAudioSource>();

            naveBody.AddComponent<NaveThrusterAudio>().SetTranslationalSource(audioSource, naveThrusterAudio);
            

            return naveBody;
        }

    }
}
