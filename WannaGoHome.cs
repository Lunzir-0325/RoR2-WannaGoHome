using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace WannaGoHome
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(GUID, modname, modver)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(CommandHelper))]
    public class WannaGoHome : BaseUnityPlugin
    {

        public const string GUID = "com.Lunzir.WannaGoHome", modname = "WannaGoHome", modver = "1.1.1";
        System.Random Random = new System.Random();
        public static int BossDefeatedCount;
        public static string SpawnCard_BluePortal = "SpawnCards/InteractableSpawnCard/iscShopPortal";
        public static string SpawnCard_GreenPortal = "SpawnCards/InteractableSpawnCard/iscMSPortal";
        public static string SpawnCard_GoldPortal = "SpawnCards/InteractableSpawnCard/iscGoldShoresPortal";
        //public static string SpawnCard_Scavbackpack = "SpawnCards/InteractableSpawnCard/iscscavbackpack";
        //public static string SpawnCard_Chest1 = "SpawnCards/InteractableSpawnCard/iscchest1";
        //public static string SpawnCard_Chest2 = "SpawnCards/InteractableSpawnCard/iscchest2";
        //public static string SpawnCard_ChestDamage = "SpawnCards/InteractableSpawnCard/isccategorychestdamage";
        //public static string SpawnCard_ChestHealing = "SpawnCards/InteractableSpawnCard/isccategorychesthealing";
        //public static string SpawnCard_ChestUtility = "SpawnCards/InteractableSpawnCard/isccategorychestutility";
        public static Dictionary<string, Map> Maps = new Dictionary<string, Map>();
        public void Awake()
        {
            ModConfig.InitConfig(Config);
            if (ModConfig.EnableMod.Value)
            {
                On.RoR2.Run.Start += Run_Start; ;
                //On.RoR2.Run.OnDestroy += Run_OnDestroy;
                On.RoR2.SceneDirector.Start += SceneDirector_Start;
                // 击败boss
                On.RoR2.Run.OnServerBossDefeated += Run_OnServerBossDefeated; 
            }
        }

        public static void InitMapsData()
        {
            Maps.Clear();
            Maps = new Dictionary<string, Map>
            {
                { "moon", new Map("moon", false) },
                { "moon2", new Map("moon2", false) },
                { "limbo", new Map("limbo", false) },
                { "voidraid", new Map("voidraid", false) },
                { "goldshores", new Map("goldshores", false) },
                { "artifactworld", new Map("artifactworld", false) }
            };
        }
        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value)
            {
                Config.Reload();
                ModConfig.InitConfig(Config);
                ModConfig.SplitMap();
                BossDefeatedCount = 0;
            }
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            if (ModConfig.EnableMod.Value)
            {
                BossDefeatedCount = 0; 
            }
            orig(self);
        }
        //private void Run_OnDestroy(On.RoR2.Run.orig_OnDestroy orig, Run self)
        //{
        //    if (ModConfig.EnableMod.Value)
        //    {
        //        BossDefeatedCount = 0;
        //        //On.RoR2.Run.Start -= Run_Start; ;
        //        //On.RoR2.Run.OnDestroy -= Run_OnDestroy;
        //        //On.RoR2.Run.OnServerBossDefeated -= Run_OnServerBossDefeated; 
        //    }
        //    orig(self);
        //}

        private void Run_OnServerBossDefeated(On.RoR2.Run.orig_OnServerBossDefeated orig, Run self, BossGroup bossGroup)
        {
            if (ModConfig.EnableMod.Value)
            {
                float maxDistance = float.PositiveInfinity, minDistance = 10f;
                if (IsCurrentMap(SceneName.MOON) || IsCurrentMap(SceneName.MOON2) || IsCurrentMap(SceneName.VOIDRAID))
                {
                    BossDefeatedCount++;
                    if (BossDefeatedCount == 1) { }
                    else if (BossDefeatedCount == 4 && (IsCurrentMap(SceneName.MOON) || IsCurrentMap(SceneName.MOON2))) // 月球地图
                    {
                        //SpawnObject(SpawnCard_BluePortal, new Vector3(-90f, 490f, 10f)); // 两个开端地图同一个位置
                        //if (IsCurrentMap(SceneName.MOON2)) SpawnObject(SpawnCard_GreenPortal, new Vector3(-90f, 490f, -10f));
                        //if (IsCurrentMap(SceneName.MOON) || IsCurrentMap(SceneName.MOON2)) BossDefeatedCount = 0;
                        StartCoroutine(DelaySpawnPortal(10, "moon"));
                    }
                    else if (BossDefeatedCount == 3 && IsCurrentMap(SceneName.VOIDRAID)) // 天文馆
                    {
                        // 第三次杀死boss 场景会随机，所以全生成。
                        StartCoroutine(DelaySpawnPortal(5, "voidraid"));
                    }
                }
                if (IsCurrentMap(SceneName.LIMBO)) // 完整之时
                {
                    //SpawnObject(SpawnCard_BluePortal, new Vector3(-30f, -8f, -15f));
                    SpawnObject(SpawnCard_BluePortal, new Vector3(18.2968f, -6.9051f, 23.8503f));
                }
                if (IsCurrentMap(SceneName.GOLDSHORES)) // 黄金海岸
                {
                    //SpawnObject(SpawnCard_BluePortal, new Vector3(-10f, -8f, 30f), new Vector3(0f, 275f, 0f));
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-10f, -8f, 30f));
                }
                if (IsCurrentMap(SceneName.ARTIFACTWORLD)) // 堡垒
                {
                    //SpawnObject(SpawnCard_BluePortal, new Vector3(0f, 11f, -92f), new Vector3(0f, 270f, 0f));
                    SpawnObject(SpawnCard_BluePortal, new Vector3(0f, 11f, -92f));
                }
                //Send($"BossDefeatedCount 打败Boss第{BossDefeatedCount}次");
            }
            orig(self, bossGroup);
        }
        private IEnumerator DelaySpawnPortal(float second, string mapName, BossGroup bossGroup = null, float maxDistance = float.PositiveInfinity, float minDistance = float.PositiveInfinity)
        {
            yield return new WaitForSeconds(second);
            if (mapName == "moon")
            {
                SpawnObject(SpawnCard_BluePortal, new Vector3(-90f, 490f, 10f), default, maxDistance, minDistance, null);
                if (IsCurrentMap(SceneName.MOON2)) SpawnObject(SpawnCard_GreenPortal, new Vector3(-90f, 490f, -10f), default, maxDistance, minDistance, null);
                if (IsCurrentMap(SceneName.MOON) || IsCurrentMap(SceneName.MOON2)) BossDefeatedCount = 0;
            }
            else if (mapName == "voidraid")
            {
                int index = Random.Next(4);
                // 地图 塞壬召唤
                if (index == 0)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3984.862f, 2.4855f, -159.6944f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4017.031f, 2.81f, -191.8f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4048.254f, 2.4434f - 159.0831f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 1)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3985.095f, 16.8331f, 184.14f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3962.959f, 16.8513f, 181.1637f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3983.042f, -0.0478f, 121.56f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 2)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3830.506f, 4.3427f, 26.3326f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3816.33f, 3.2562f, 0.4805f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3856.461f, -0.0671f, 12.055f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 3)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3825.777f, 33.0321f, -39.2592f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3873.801f, 20.7266f, -63.7816f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3838.731f, 16.7501f, -93.0241f), default, maxDistance, minDistance, null, false);
                }
                index = Random.Next(4);
                // 地图 巨大平原
                if (index == 0)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-153.5481f, 11.614f, 3976.873f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-186.8349f, 11.614f, 4003.584f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-157.0579f, 11.614f, 4007.065f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 1)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(136.0411f, 55.8862f, 3975.073f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(147.6504f, 56.9738f, 4012.781f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(146.9672f, 62.5851f, 4034.888f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 2)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(35.0495f, 6.904f, 4111.528f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(51.8169f, 6.0754f, 4104.176f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(20.0302f, 6.8112f, 4118.5f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 3)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-85.6391f, 0.8241f, 3849.736f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-117.7041f, 1.8856f, 3887.454f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-86.1519f, 5.2133f, 3911.532f), default, maxDistance, minDistance, null, false);
                }
                index = Random.Next(6);
                // 地图 阿菲利安避难所
                if (index == 0)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4000.247f, 4.6284f, 4158.451f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4018.495f, 4.769f, 4156.4f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3980.958f, 4.7455f, 4156.712f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 1)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4074.52f, 27.8466f, 4163.931f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4125.823f, 23.3243f, 4116.826f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4159.071f, 17.8592f, 4068.031f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 2)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3922.275f, 24.6852f, 3836.205f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3873.899f, 19.7325f, 3874.084f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3837.624f, 15.6205f, 3916.829f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 3)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4175.244f, 4.6327f, 4003.311f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4126.389f, 15.3991f, 3900.852f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4175.572f, 15.2136f, 3943.008f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 4)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3809.811f, 2.9216f, 4124.213f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3811.864f, 38.0131f, 4122.1f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3821.534f, 64.696f, 4115.31f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 5)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4003.322f, 41.1283f, 3837.349f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3973.9f, 5.7533f, 3845.087f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4036.8f, 3.1848f, 3845.494f), default, maxDistance, minDistance, null, false);
                }
                index = Random.Next(3);
                // 地图 深渊
                if (index == 0)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(164.7265f, 18.2982f, -4064.977f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(120.3534f, 22.231f, -4133.031f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(15.6985f, 40.8541f, -4175.558f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 1)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(98.9261f, 25.1039f, -4044.661f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-62.9775f, 30.9852f, -3905.364f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(36.5528f, 39.3514f, -3893.232f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 2)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-85.202f, 5.3124f, -4088.821f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(97.7129f, 3.0639f, -3947.782f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4.2404f, 0.5868f, -3857.179f), default, maxDistance, minDistance, null, false);
                }
                index = Random.Next(4);
                // 地图 偏远栖所
                if (index == 0)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4055.791f, 18.8021f, -4167.633f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3996.855f, 21.5257f, -4153.481f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3971.708f, -1.3039f, -4180.095f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 1)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4167.088f, 58.8958f, -3900.667f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4168.866f, 42.5595f, -3947.548f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4171.823f, 36.0896f, -3980.305f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 2)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-4129.632f, -1.4249f, -3956.921f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-4108.375f, -1.1653f, -3902.529f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-4164.92f, -1.2066f, -3922.084f), default, maxDistance, minDistance, null, false);
                }
                else if (index == 3)
                {
                    SpawnObject(SpawnCard_BluePortal, new Vector3(-3844.365f, 7.3781f, -4070.135f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GreenPortal, new Vector3(-3913.457f, 36.064f, -4150.166f), default, maxDistance, minDistance, null, false);
                    SpawnObject(SpawnCard_GoldPortal, new Vector3(-3844.365f, 7.3781f, -4070.135f), default, maxDistance, minDistance, null, false);
                }
                ShowPortalInfo(Portal.Blue);
                ShowPortalInfo(Portal.Green);
                ShowPortalInfo(Portal.Gold);
                BossDefeatedCount = 0;
            }
        }
        private void SpawnObject(string cardName, Vector3 position, Vector3 rotaion = default, float maxDistance = float.PositiveInfinity, float minDistance = 10f, Transform transform = null, bool showInfo = true)
        {
            //Debug.Log("进入 SpawnObject");
            if (cardName.EndsWith("MSPortal") && !ModConfig.EnableGreenPortal.Value)
            {
                return;
            }
            else if (cardName.EndsWith("GoldShoresPortal") && !ModConfig.EnableGoldPortal.Value)
            {
                return;
            }
            //Debug.Log($"生成{cardName}");
            SpawnCard card = LegacyResourcesAPI.Load<SpawnCard>(cardName);
            GameObject obj = card.DoSpawn(position, Quaternion.identity, new DirectorSpawnRequest(card, new DirectorPlacementRule()
            {
                //minDistance = minDistance,
                //maxDistance = maxDistance,
                //placementMode = transform == null ? DirectorPlacementRule.PlacementMode.Direct : DirectorPlacementRule.PlacementMode.NearestNode,
                placementMode = DirectorPlacementRule.PlacementMode.Direct
                //position = position,
                //spawnOnTarget = transform == null ? base.transform : this.transform,
            }, Run.instance.runRNG)).spawnedInstance;
            obj.transform.eulerAngles = rotaion;
            //GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>(cardName), new DirectorPlacementRule
            //{
            //    minDistance = minDistance,
            //    maxDistance = maxDistance,
            //    placementMode = transform == null ? DirectorPlacementRule.PlacementMode.Direct : DirectorPlacementRule.PlacementMode.NearestNode,
            //    position = position,
            //    spawnOnTarget = transform,
            //}, Run.instance.stageRng));
            //gameObject.transform.eulerAngles = rotaion;
            if (showInfo)
            {
                if (cardName.EndsWith("ShopPortal"))
                {
                    ShowPortalInfo(Portal.Blue);
                }
                else if (cardName.EndsWith("MSPortal"))
                {
                    ShowPortalInfo(Portal.Green);
                }
                else if (cardName.EndsWith("GoldShoresPortal"))
                {
                    ShowPortalInfo(Portal.Gold);
                } 
            }
        }
        private void ShowPortalInfo(Portal portal)
        {
            switch (portal)
            {
                case Portal.Blue:
                    Send("PORTAL_SHOP_OPEN");
                    break;
                case Portal.Green:
                    Send("PORTAL_MS_OPEN");
                    break;
                case Portal.Gold:
                    Send("PORTAL_GOLDSHORES_OPEN");
                    break;
                default:
                    break;
            }
        }

        private bool IsCurrentMap(string mapName)
        {
            if (SceneManager.GetActiveScene().name == mapName) // 当前场景
            {
                if (Maps.TryGetValue(mapName, out Map map)) // 是否启用该地图
                {
                    return map.Enable;
                } 
            }
            return false;
        }
        public static void Send(string message)
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = message
            });
        }

        internal class SceneName
        {
            public static string MOON = "moon";
            public static string MOON2 = "moon2";
            public static string BAZAAR = "bazaar";
            public static string MYSTERYSPACE = "mysteryspace";
            public static string LIMBO = "limbo";
            public static string GOLDSHORES = "goldshores";
            public static string ARENA = "arena";
            public static string ARTIFACTWORLD = "artifactworld";
            public static string VOIDSTAGE = "voidstage";
            public static string VOIDRAID = "voidraid";
        }
        
        internal enum Portal
        {
            Blue,Green,Gold
        }
    }
    public class Map
    {
        public Map(string name, bool enable)
        {
            Name = name;
            Enable = enable;
        }

        public string Name { get; set; }
        public bool Enable { get; set; }
    }
    class ModConfig
    {
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<string> SetMaps;
        public static ConfigEntry<bool> EnableGreenPortal;
        public static ConfigEntry<bool> EnableGoldPortal;
        //public static ConfigEntry<DirectorPlacementRule.PlacementMode> PlacementMode;
        public static void InitConfig(ConfigFile config)
        {
            EnableMod = config.Bind("Setting设置", "EnableMod", true, "Enable Mod. In some maps, a portal appears when a boss is defeated.\n启用模组，此mod的用途就是在一些场景打完boss强制出现传送门回月店");
            if (EnableMod.Value)
            {
                SetMaps = config.Bind("Setting设置", "SetMap", "moon2,voidraid,goldshores,limbo,artifactworld", "Set up available maps.\n设置可用的地图，用逗号隔开。\nmoo2 = 月球，voidraid = 天文馆，goldshores = 黄金海岸，limbo = 完整之时，artifactworld = 神器空间");
                EnableGreenPortal = config.Bind("Setting设置", "EnableGreenPortal", true, "The green(ms) portal will appear on the moon and planetarium.\n启用青蓝色传送门，会在月球和天文馆出现。");
                EnableGoldPortal = config.Bind("Setting设置", "EnableGoldPortal", true, "The gold portal will appear on the planetarium.\n启用金色传送门，会在天文馆出现。"); 
            }
            //PlacementMode = config.Bind("0 Setting设置", "PlacementMode", DirectorPlacementRule.PlacementMode.NearestNode, "摆放方式");
            SplitMap();
        }

        public static void SplitMap()
        {
            WannaGoHome.InitMapsData();
            string[] maps = SetMaps.Value.Split(',');
            for (int i = 0; i < maps.Length; i++)
            {
                if (WannaGoHome.Maps.TryGetValue(maps[i].Trim().ToLower(), out Map map))
                {
                    map.Enable = true;
                }

            }
        }
    }
}
