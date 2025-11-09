using System;
using PlazmaGames.Core;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Animation;
using PlazmaGames.Core.Utils;
using Unity.Hierarchy;
using LJ2025.MonoSystems;
using Time = UnityEngine.Time;
using PlazmaGames.UI;
using LJ2025.UI;

namespace LJ2025
{
    public enum GameState
    {
        Start,
        LeaveForWork,
        ServeGuest,
        GetTrashCan,
        GoCleanRoom,
        FixWater,
        WaitCheckout,
        CheckOnGuests,
        GuestsDead0,
        GuestsDead1,
        GuestsDead2,
        GuestsDead3,
    }

    public static class GameStateExt
    {
        public static bool IsBefore(this GameState state, GameState to)
        {
            return (int)state < (int)to;
        }
        public static bool IsAfter(this GameState state, GameState to)
        {
            return (int)state > (int)to;
        }

        public static bool IsEqual(this GameState state, GameState to)
        {
            return (int)state == (int)to;
        }
    }
    
    public class GameLogicMonoSystem : MonoBehaviour, IGameLogicMonoSystem
    {
        [SerializeField] private float _globalTimeScale = 1;

        private IDialogueMonoSystem _dialogueMs;
        private IScreenEffectMonoSystem _screenEffectMs;
        private ITaskMonoSystem _taskMs;
        private DateTime _date = new DateTime(2012, 3, 3, 17, 0, 0);
        private float _timeScale = 10;

        private ResetableState[] _states;

        private Scheduler _scheduler = new();
        public Scheduler Scheduler() => _scheduler;

        private int _act = 0;
        private GameState _gameState = LJ2025.GameState.Start;

        private Player.Controller _player;
        private Player.Interactor _playerInteractor;
        private Player.Inspector _playerInspector;
        private Player.ObjectMover _playerMover;

        private HashSet<string> _inRange = new();

        private class Refs
        {
            public Chair startChair;
            public Phone homePhone;
            public Pather dad;
            public Dictionary<string, GameObject> scenes;
            public Chair busStopChair;
            public BusDoor busDoor;
            public MoveAlongSpline busMover;
            public Phone officePhone;
            public VendingMachine vendingMachine;
            public TwoWayDoor officeDoors;
            public Pather guy1Pather;
            public Pather guy2Pather;
            public Pather guy3Pather;
            public Grabber guy1Grabber;
            public GuyDoor guy1Door;
            public GuyDoor guy2Door;
            public TwoWayDoor guy2BathroomDoor;
            public TwoWayDoor guy3Door;
            public Pather manager;
            public GameObject trashBag;
            public Transform curtains;
            public GameObject deathRooms;
            public Transform showerZoomLocation;
            public Transform foodZoomLocation;
            public Transform heartAttackLocation;
            public HeartAttack heartAttack;
            public TrashBin roomTrashBin;
            public LightFlicker2 finalLight;
            public LightFlicker2 maintenanceLight;
            public TwoWayDoor maintenanceDoor;
            public Peeker peeker;
            public ShowerController murderBath;
            public ShowerKnob murderKnob;
            public TVController tv;
            public LightFlicker2 room1Light;
            public LightFlicker2 room2Light;
            public LightFlicker2 room3Light;
            public AudioSource ambiance;
            public AudioSource spookSound;
            public AudioSource wind;
            public InspectableObject connect4;
        }

        private void Start()
        {
            _screenEffectMs = GameManager.GetMonoSystem<IScreenEffectMonoSystem>();
            _dialogueMs = GameManager.GetMonoSystem<IDialogueMonoSystem>();
            _taskMs = GameManager.GetMonoSystem<ITaskMonoSystem>();
            _player = GameObject.FindAnyObjectByType<Player.Controller>();
            _playerInteractor = _player.GetComponent<Player.Interactor>();
            _playerInspector = _player.GetComponent<Player.Inspector>();
            _playerMover = _player.GetComponent<Player.ObjectMover>();
            
            _refs.startChair = GameObject.FindWithTag("StartChair").GetComponent<Chair>();
            _refs.homePhone = GameObject.FindWithTag("HomePhone").GetComponent<Phone>();
            _refs.dad = GameObject.FindWithTag("Dad").GetComponent<Pather>();
            Transform sceneHolder = GameObject.FindWithTag("SceneHolder").transform;
            _refs.scenes = new Dictionary<string, GameObject>();
            foreach (Transform scene in sceneHolder)
            {
                _refs.scenes.Add(scene.gameObject.name, scene.gameObject);
            }

            _refs.ambiance = GameObject.FindWithTag("Ambiance").GetComponent<AudioSource>();
            _refs.wind = GameObject.FindWithTag("Wind").GetComponent<AudioSource>();
            _refs.spookSound = GameObject.FindWithTag("SpookSound").GetComponent<AudioSource>();
            
            _refs.busStopChair = GameObject.FindWithTag("BusStopChair").GetComponent<Chair>();
            _refs.busDoor = GameObject.FindWithTag("Bus").GetComponentInChildren<BusDoor>();
            _refs.busMover = GameObject.FindWithTag("Bus").GetComponent<MoveAlongSpline>();
            
            _refs.officePhone = GameObject.FindWithTag("OfficePhone").GetComponent<Phone>();
            _refs.vendingMachine = GameObject.FindAnyObjectByType<VendingMachine>();
        
            _refs.officeDoors = GameObject.FindWithTag("OfficeDoor").GetComponent<TwoWayDoor>();
            _refs.guy1Pather = GameObject.FindWithTag("Guy1").GetComponent<Pather>();
            _refs.guy1Grabber = GameObject.FindWithTag("Guy1").GetComponent<Grabber>();
            _refs.guy1Pather.gameObject.SetActive(false);
            _refs.guy2Pather = GameObject.FindWithTag("Guy2").GetComponent<Pather>();
            _refs.guy2Pather.gameObject.SetActive(false);
            _refs.guy3Pather = GameObject.FindWithTag("Guy3").GetComponent<Pather>();
            _refs.guy3Pather.gameObject.SetActive(false);
            _refs.guy1Door = GameObject.FindWithTag("Guy1Door").GetComponent<GuyDoor>();
            _refs.guy2Door = GameObject.FindWithTag("Guy2Door").GetComponent<GuyDoor>();
            _refs.guy2BathroomDoor = GameObject.FindWithTag("Guy2BathroomDoor").GetComponent<TwoWayDoor>();
            _refs.guy3Door = GameObject.FindWithTag("Guy3Door").GetComponent<GuyDoor>();
            _refs.guy3Door.Lock();
            _refs.manager = GameObject.FindWithTag("Manager").GetComponent<Pather>();
            _refs.manager.gameObject.SetActive(false);
            _refs.trashBag = GameObject.FindWithTag("TrashBag");
            _refs.trashBag.SetActive(false);
            _refs.curtains = GameObject.FindWithTag("Curtains").transform;
            _refs.deathRooms = GameObject.FindWithTag("DeathRooms");
            _refs.showerZoomLocation = GameObject.FindWithTag("ShowerZoom").transform;
            _refs.foodZoomLocation = GameObject.FindWithTag("FoodZoom").transform;
            _refs.heartAttackLocation = GameObject.FindWithTag("HeartAttack").transform;
            _refs.heartAttack = GameObject.FindAnyObjectByType<HeartAttack>();
            _refs.roomTrashBin = GameObject.FindWithTag("RoomTrashBin").GetComponent<TrashBin>();
            _refs.finalLight = GameObject.FindWithTag("FinalDoorLight").GetComponent<LightFlicker2>();
            _refs.finalLight.enabled = false;
            _refs.finalLight.GetComponent<Light>().enabled = true;
            _refs.maintenanceLight = GameObject.FindWithTag("MaintenanceLight").GetComponent<LightFlicker2>();
            _refs.maintenanceLight.enabled = false;
            _refs.maintenanceDoor = GameObject.FindWithTag("MaintenanceDoor").GetComponent<TwoWayDoor>();
            _refs.murderBath = GameObject.FindWithTag("MurderBath").GetComponent<ShowerController>();
            _refs.murderKnob = GameObject.FindWithTag("MurderKnob").GetComponent<ShowerKnob>();
            _refs.tv = GameObject.FindWithTag("TV").GetComponent<TVController>();
            _refs.room1Light = GameObject.FindWithTag("Room1Light").GetComponent<LightFlicker2>();
            _refs.room1Light.enabled = false;
            _refs.room1Light.GetComponent<Light>().enabled = true;
            _refs.room2Light = GameObject.FindWithTag("Room2Light").GetComponent<LightFlicker2>();
            _refs.room2Light.enabled = false;
            _refs.room2Light.GetComponent<Light>().enabled = true;
            _refs.room3Light = GameObject.FindWithTag("Room3Light").GetComponent<LightFlicker2>();
            _refs.room3Light.enabled = false;
            _refs.room3Light.GetComponent<Light>().enabled = true;
            _refs.connect4 = GameObject.FindWithTag("Connect4").GetComponent<InspectableObject>();

            _refs.deathRooms.SetActive(false);
            
            _states = GameObject.FindObjectsByType<ResetableState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var rs in _states) rs.InitState();

            _refs.peeker = GameObject.FindAnyObjectByType<Peeker>();
            _refs.peeker.gameObject.SetActive(false);
            DoPeek();
        }

        private void DoPeek()
        {
            _scheduler.When(() => IsInRange("BackOffice"))
                .Then(_ => _scheduler.Wait(UnityEngine.Random.Range(3, 6)))
                .Then(_ =>
                {
                    if (IsInRange("BackOffice") && UnityEngine.Random.Range(0, 2) == 0) _refs.peeker.Peek(0.3f, 1.7f);
                })
                .Then(_ => _scheduler.Wait(7))
                .Then(_ =>
                {
                    DoPeek();
                });
        }


        private Refs _refs = new();

        public Player.ObjectMover GetObjectMover() => _playerMover;
        
        public int Act() => _act;
        public GameState GameState() => _gameState;

        public System.DateTime CurrentDate() => _date;
        public int CurrentYear() => 2012 + _act - 1;
        public int CurrentDay() => 3;
        public int CurrentHour() => _date.Hour;
        public string CurrentTime() => _date.ToString("hh:mm");
        
        private void SetDate() => _date = new DateTime(2012, 3, 3 + _act, 17, 0, 0);
        
        public void TriggerEvent(string eventName, Transform by)
        {
            Debug.Log("Event: '" + eventName + "'");
            switch (eventName)
            {
                case "Begin":
                {
                    foreach (var (sceneName, sceneObj) in _refs.scenes)
                    {
                        if (sceneName != "House") sceneObj.SetActive(false);
                    }
                    _player.TeleportToChair(_refs.startChair);
                    _player.LockHead();
                    _scheduler.Wait(1)
                        .Then(_ => _dialogueMs.StartDialoguePromise("StartingMonologue"))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Click 'Space' To Get Up");
                            _player.UnlockHead();
                        })
                        .Then(_ => _scheduler.Wait(5f))
                        .Then(_ => { _taskMs.EndTask(); })
                        .Then(_ => _scheduler.Wait(UnityEngine.Random.Range(15f, 20f)))
                        .Then(_ => { _dialogueMs.StartDialoguePromise("GotCall", passive: true); })
                        .Then(_ => _refs.homePhone.Ring())
                        .Then(_ => _dialogueMs.StartDialoguePromise("NewJobPhoneCall"))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Leave The House");
                            _gameState = LJ2025.GameState.LeaveForWork;
                        });
                    
                    break;
                }

                case "LeaveForWork":
                {
                    _taskMs.EndTask();
                    LJ2025GameManager.LockMovement = true;
                    _screenEffectMs.Fadeout(1)
                        .Then(_ => _screenEffectMs.FadeoutText("Bus Stop", 1))
                        .Then(_ =>
                        {
                            TriggerEvent("StartBusScene", transform);
                        });
                    break;
                }

                case "StartBusScene":
                {
                    _refs.scenes["House"].SetActive(false);
                    _refs.scenes["BusStop"].SetActive(true);
                    _player.TeleportToChair(_refs.busStopChair);
                    _player.LockHead();
                    _player.SetIndoors(false);
                    
                    _screenEffectMs.FadeinText(2)
                        .Then(_ => _screenEffectMs.Fadein(2))
                        .Then(_ =>
                        {
                            LJ2025GameManager.LockMovement = false;
                        })
                        .Then(_ => _refs.dad.Next())
                        .Then(_ => _dialogueMs.StartDialoguePromise("BusStopTalk"))
                        .Then(_ =>
                        {
                            _refs.busMover.Continue();
                        })
                        .Then(_ => _scheduler.When(() => !_refs.busMover.IsMoving()))
                        .Then(_ =>
                        {
                            _refs.busDoor.Open();
                        })
                        .Then(_ => _scheduler.Wait(1))
                        .Then(_ => _dialogueMs.StartDialoguePromise("BusIsHere"))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Get On The Bus");
                            _player.UnlockHead();
                        });
                    break;
                }

                case "GoOnBus":
                {
                    _taskMs.EndTask();
                    LJ2025GameManager.LockMovement = true;
                    _player.LockHead();
                    _player.DetachHeadLookAt(_refs.dad.transform.position.AddY(1.4f));
                    _scheduler.Wait(1.4f)
                        .Then(_ => _dialogueMs.StartDialoguePromise("ArentYouComing"))
                        .Then(_ =>
                        {
                            _player.AttachHead();
                        })
                        .Then(_ => _scheduler.When(() => !_player.HasDetachedHead()))
                        .Then(_ => _screenEffectMs.Fadeout(1))
                        .Then(_ => _screenEffectMs.FadeoutText("Motel", 1))
                        .Then(_ =>
                        {
                            TriggerEvent("StartMotelScene", transform);
                        });
                    break;
                }

                case "StartMotelScene":
                {
                    _refs.scenes["BusStop"].SetActive(false);
                    _refs.scenes["Motel"].SetActive(true);
                    _refs.ambiance.Play();
                    _refs.wind.Play();
                    _player.Teleport(_refs.scenes["Motel"].transform.Find("StartLocation"));
                    _player.UnlockHead();
                    _screenEffectMs.FadeinText(2)
                        .Then(_ => _screenEffectMs.Fadein(2))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Go To The Front Office");
                            LJ2025GameManager.LockMovement = false;
                        })
                        .Then(_ => _scheduler.When(() => IsInRange("Office")))
                        .Then(_ =>
                        {
                            _taskMs.EndTask();
                            _refs.officeDoors.Close();
                        })
                        .Then(_ => _scheduler.Wait(UnityEngine.Random.Range(3f, 10f)))
                        .Then(_ => _refs.officePhone.Ring())
                        .Then(_ => _dialogueMs.StartDialoguePromise("IntroductionCall"))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Serve Any Customers That Come In.");
                            _gameState = LJ2025.GameState.ServeGuest;
                            _refs.guy1Pather.gameObject.SetActive(true);
                        })
                        .Then(_ => _scheduler.Wait(UnityEngine.Random.Range(5f, 20f)))
                        .Then(_ => _refs.guy1Pather.Next())
                        .Then(_ => _refs.officeDoors.OpenThenClose(_refs.guy1Pather.transform, 2))
                        .Then(_ => _refs.guy1Pather.Next());
                    break;
                }

                case "Guy1Talk":
                {
                    _dialogueMs.StartDialoguePromise("Guy1GetRoom")
                        .Then(_ => _refs.guy1Pather.GetComponent<Guy>().SetIsInteractable(false))
                        .Then(_ => _refs.guy1Pather.Next())
                        .Then(_ => _scheduler.Wait(2))
                        .Then(_ =>
                        {
                            _taskMs.EndTask();
                        })
                        .Then(_ => _dialogueMs.StartDialoguePromise("Guy1VendingMachineEmpty"))
                        .Then(_ =>
                        {
                            _refs.connect4.SetInteractable(true);
                        })
                        .Then(_ =>
                        {
                            _refs.vendingMachine.TaskEnabled = true;
                            _taskMs.StartTask("Refill The Vending Machine", _refs.vendingMachine.GetMaxCount());
                        })
                        .Then(_ => _refs.guy1Pather.Next())
                        .Then(_ => _scheduler.When(() => _refs.vendingMachine.IsStocked()))
                        .Then(_ =>
                        {
                            _refs.vendingMachine.TaskEnabled = false;
                            _taskMs.EndTask();
                        })
                        .Then(_ => _scheduler.Wait(1.5f))
                        .Then(_ => _refs.guy1Pather.Next())
                        .Then(_ => _scheduler.Wait(1.5f))
                        .Then(_ => _dialogueMs.StartDialoguePromise("Guy1VendingChoose"))
                        .Then(_ =>
                        {
                            _refs.connect4.SetInteractable(false);
                        })
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Serve Any Customers That Come In.");
                        })
                        .Then(_ => _scheduler.Wait(1.5f))
                        .Then(_ => _refs.guy1Grabber.Grab(_refs.vendingMachine.RemoveItem()))
                        .Then(_ => _refs.guy1Pather.Next())
                        .Then(_ => _refs.officeDoors.OpenThenClose(_refs.guy1Pather.transform, 2))
                        .Then(_ =>
                        {
                            _refs.guy1Pather.Next()
                                .Then(_ => _refs.guy1Door.OpenThenClose(_refs.guy1Pather.transform, 2))
                                .Then(_ => _refs.guy1Pather.Next())
                                .Then(_ =>
                                {
                                    _refs.curtains.GetChild(1).localScale = new Vector3(1, 1, 1);
                                    _refs.guy1Door.Lock();
                                    _refs.guy1Pather.gameObject.SetActive(false);
                                });
                                _refs.guy3Pather.gameObject.SetActive(true);
                        })
                        .Then(_ => _refs.guy3Pather.Next())
                        .Then(_ => _refs.officeDoors.OpenThenClose(_refs.guy3Pather.transform, 2))
                        .Then(_ => _refs.guy3Pather.Next());
                    break;
                }

                case "Guy3Talk":
                {
                    _gameState = LJ2025.GameState.GetTrashCan;
                    _taskMs.EndTask();
                    _dialogueMs.StartDialoguePromise("Guy3Leaving")
                        .Then(_ => _refs.guy3Pather.GetComponent<Guy>().SetIsInteractable(false))
                        .Then(_ =>
                        {
                            _refs.guy3Door.Unlock();
                            _taskMs.StartTask("Go To Room 3");
                            _scheduler.When(() => IsInRange("Guy3Found"))
                            .Then(_ => {
                                _taskMs.EndTask();
                                _refs.roomTrashBin.TaskEnabledRoom = true;
                                _taskMs.StartTask("Pick Up Trash", _refs.roomTrashBin.RequiredTrash); 
                            });
                        })
                        .Then(_ => _refs.guy3Pather.Next())
                        .Then(_ => _refs.officeDoors.OpenThenClose(_refs.guy3Pather.transform, 2))
                        .Then(_ => _refs.guy3Pather.Next())
                        .Then(_ =>
                        {
                            _refs.guy3Pather.gameObject.SetActive(false);
                        });
                    break;
                }
                
                case "AllTrashCleared":
                {
                    _refs.roomTrashBin.TaskEnabledRoom = false;
                    _taskMs.EndTask();
                    _taskMs.StartTask("Go To The Alleyway To Throw Out The Trash");
                    _refs.trashBag.SetActive(true);

                    _gameState = LJ2025.GameState.ServeGuest;
                    break;
                }

                case "TrashThrownOut":
                {
                    _taskMs.EndTask();
                    _taskMs.StartTask("Serve Any Customers That Come In.");
                    _dialogueMs.StartDialoguePromise("GetBackToOffice", passive: true);
                    _refs.guy2Pather.gameObject.SetActive(true);
                    break;
                }

                case "Guy2Talk":
                {
                    _dialogueMs.StartDialoguePromise("Guy2GetRoom")
                        .Then(_ => _refs.guy2Pather.GetComponent<Guy>().SetIsInteractable(false))
                        .Then(_ => _refs.guy2Pather.Next())
                        .Then(_ => _refs.officeDoors.OpenThenClose(_refs.guy2Pather.transform, 2))
                        .Then(_ => _refs.guy2Pather.Next())
                        .Then(_ => _refs.guy2Door.OpenThenClose(_refs.guy2Pather.transform, 2))
                        .Then(_ => _refs.guy2Pather.Next())
                        .Then(_ =>
                        {
                            _refs.curtains.GetChild(0).localScale = new Vector3(1, 1, 1);
                            _refs.guy2Door.Lock();
                            _refs.guy2Pather.gameObject.SetActive(false);
                        })
                        .Then(_ => _scheduler.Wait(4))
                        .Then(_ => _refs.officePhone.Ring())
                        .Then(_ => _dialogueMs.StartDialoguePromise("WaterCold"))
                        .Then(_ =>
                        {
                            _taskMs.StartTask("Find The Maintenance Room");
                            _scheduler.When(() => IsInRange("InMaintenance"))
                            .Then(_ => {
                                _taskMs.EndTask();
                                _taskMs.StartTask("Turn On Hot Water Valve");
                            });
                            _gameState = LJ2025.GameState.FixWater;
                        });
                    break;
                }

                case "WaterFixed":
                    {
                        if (_gameState != LJ2025.GameState.FixWater) break;
                        _taskMs.EndTask();
                        _dialogueMs.StartDialoguePromise("WaterFixed")
                            .Then(_ =>
                            {
                                _taskMs.StartTask("Return To Office");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Office")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                            })
                            .Then(_ => _scheduler.Wait(5))
                            .Then(_ => _screenEffectMs.Fadeout(1))
                            .Then(_ => _screenEffectMs.FadeoutText("Checkout Time\n6:00 AM", 1))
                            .Then(_ => _screenEffectMs.FadeinText(1))
                            .Then(_ => _screenEffectMs.Fadein(1))
                            .Then(_ =>
                            {
                                _refs.curtains.GetChild(0).localScale = new Vector3(1, 1, 1);
                                _refs.curtains.GetChild(1).localScale = new Vector3(1, 1, 1);
                                _refs.curtains.GetChild(2).localScale = new Vector3(1, 1, 1);
                                _refs.deathRooms.SetActive(true);
                                _refs.murderBath.Toggle();
                                _refs.murderKnob.SetInteractable();
                                _gameState = LJ2025.GameState.WaitCheckout;

                                _refs.guy1Door.Close();
                                _refs.guy2Door.Close();
                                _refs.guy3Door.Close();
                                _refs.maintenanceDoor.Close();
                                _refs.maintenanceDoor.Lock();
                                _refs.guy1Door.Lock();
                                _refs.guy2Door.Lock();
                                _refs.guy3Door.Lock();

                                foreach (LightFlicker2 light in GameObject.FindObjectsByType<LightFlicker2>(FindObjectsSortMode.None))
                                {
                                    light.gameObject.SetActive(false);
                                }
                                _refs.room1Light.gameObject.SetActive(true);
                                _refs.room2Light.gameObject.SetActive(true);
                                _refs.room3Light.gameObject.SetActive(true);

                                _refs.finalLight.gameObject.SetActive(false);
                                _refs.maintenanceLight.gameObject.SetActive(true);
                                _refs.maintenanceLight.enabled = true;

                                _refs.guy1Door.Lock();
                                _refs.guy2Door.Lock();
                                _refs.guy3Door.Lock();

                                _taskMs.StartTask("Wait for Guests to Checkout");
                            })
                            .Then(_ => _scheduler.Wait(2))
                            .Then(_ => _dialogueMs.StartDialoguePromise("CheckoutTime", passive: true))
                            .Then(_ => _scheduler.Wait(2))
                            .Then(_ => _dialogueMs.StartDialoguePromise("CheckoutTime", passive: true))
                            .Then(_ => _scheduler.Wait(2))
                            .Then(_ => _dialogueMs.StartDialoguePromise("CheckoutTime", passive: true))
                            .Then(_ => _scheduler.Wait(2))
                            .Then(_ => _dialogueMs.StartDialoguePromise("CheckoutTime", passive: true))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 1");
                                _gameState = LJ2025.GameState.CheckOnGuests;
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Door1Knocked")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 2");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Door2Knocked")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 3");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Door3Knocked")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                            })
                            .Then(_ => _scheduler.Wait(3))
                            .Then(_ => _dialogueMs.StartDialoguePromise("GetMasterKey"))
                            .Then(_ =>
                            {
                                _taskMs.StartTask("Locate Master Key in Front Office");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("MasterKey")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 1");
                            })
                            .Then(_ =>
                            {
                                _refs.room1Light.enabled = true;
                                _refs.guy2Door.Unlock();
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Guy1Found")))
                            .Then(_ =>
                            {
                                float amVolFrom = _refs.ambiance.volume;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.ambiance.volume = Mathf.Lerp(amVolFrom, 1.0f, t),
                                        () => { },
                                        true);
                                float windVolFrom = _refs.wind.volume;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.wind.volume = Mathf.Lerp(windVolFrom, 0.6f, t),
                                        () => { },
                                        true);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(1, 1.3f, t),
                                        () => { },
                                        true);
                            })
                            .Then(_ => _scheduler.When(() => _refs.guy2BathroomDoor.IsOpen()))
                            .Then(_ => _scheduler.Wait(0.2f))
                            .Then(_ =>
                            {
                                _player.LockHead();
                                LJ2025GameManager.LockMovement = true;
                                _player.DetachHead(_refs.showerZoomLocation);
                                float from = _refs.ambiance.pitch;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        1.5f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(from, 0.73f, t),
                                        () => { },
                                        true);
                                float windVolFrom = _refs.wind.volume;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.wind.volume = Mathf.Lerp(windVolFrom, 1.5f, t),
                                        () => { },
                                        true);

                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        4.0f,
                                        t =>
                                        {
                                            _screenEffectMs.SetRedShift(Mathf.Lerp(0, 0.33f, t));
                                            _screenEffectMs.SetStaticLevel(Mathf.Lerp(0, 0.33f, t) * 0.5f);
                                            _screenEffectMs.SetScreenRoundness(t);
                                            _screenEffectMs.SetScreenVignetteOpacity(Mathf.Lerp(1, 0.7f, t));
                                        },
                                        () => { },
                                        true);

                            })
                            .Then(_ => _scheduler.Wait(6))
                            .Then(_ =>
                            {
                                _player.UnlockHead();
                                LJ2025GameManager.LockMovement = false;
                                _player.AttachHeadImmediately();
                                _refs.room1Light.gameObject.SetActive(false);
                                _refs.room2Light.enabled = true;
                            })
                            .Then(_ => _dialogueMs.StartDialoguePromise("FoundShower"))
                            .Then(_ =>
                            {
                                _refs.guy1Door.Unlock();
                                float from = _refs.ambiance.pitch;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        6.0f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(from, 1.40f, t),
                                        () => { },
                                        true);
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 2");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Guy2Found")))
                            .Then(_ =>
                            {
                                _player.LockHead();
                                LJ2025GameManager.LockMovement = true;
                                _player.DetachHead(_refs.foodZoomLocation);
                                float from = _refs.ambiance.pitch;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(from, 0.7f, t),
                                        () => { },
                                        true);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        4.0f,
                                        t =>
                                        {
                                            _screenEffectMs.SetRedShift(Mathf.Lerp(0.33f, 0.66f, t));
                                            _screenEffectMs.SetStaticLevel(Mathf.Lerp(0.33f, 0.66f, t) * 0.5f);
                                            _screenEffectMs.SetScreenVignetteOpacity(Mathf.Lerp(0.7f, 0.5f, t));
                                        },
                                        () => { },
                                        true);
                            })
                            .Then(_ => _scheduler.Wait(6))
                            .Then(_ =>
                            {
                                _player.UnlockHead();
                                LJ2025GameManager.LockMovement = false;
                                _player.AttachHeadImmediately();
                            })
                            .Then(_ => _dialogueMs.StartDialoguePromise("FoundFood"))
                            .Then(_ =>
                            {
                                _refs.guy3Door.Unlock();
                                _refs.room2Light.gameObject.SetActive(false);
                                _refs.room3Light.enabled = true;
                                float from = _refs.ambiance.pitch;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        6.0f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(from, 1.40f, t),
                                        () => { },
                                        true);
                                _taskMs.EndTask();
                                _taskMs.StartTask("Check on Guest in Room 3");
                            })
                            .Then(_ => _scheduler.When(() => IsInRange("Guy3Found")))
                            .Then(_ => _scheduler.When(() => _refs.guy3Door.IsOpen()))
                            .Then(_ =>
                            {
                                _refs.guy3Door.Close();
                                _player.LockHead();
                                LJ2025GameManager.LockMovement = true;
                                _player.DetachHead(_refs.heartAttackLocation);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
                                float from = _refs.ambiance.pitch;
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        0.6f,
                                        t => _refs.ambiance.pitch = Mathf.Lerp(from, 0.7f, t),
                                        () => { },
                                        true);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                    .RequestAnimation(
                                        this,
                                        4.0f,
                                        t =>
                                        {
                                            _screenEffectMs.SetRedShift(Mathf.Lerp(0.66f, 1, t));
                                            _screenEffectMs.SetStaticLevel(Mathf.Lerp(0.66f, 1, t) * 0.5f);
                                            _screenEffectMs.SetScreenVignetteOpacity(Mathf.Lerp(0.5f, 0.3f, t));
                                        },
                                        () => { },
                                        true);
                            })
                            .Then(_ => _scheduler.Wait(2))
                            .Then(_ => _dialogueMs.StartDialoguePromise("HeartAttack"))
                            .Then(_ =>
                            {
                                _refs.heartAttack.StartAttack(4);
                            })
                            .Then(_ => _scheduler.Wait(7))
                            .Then(_ =>
                            {
                                _player.UnlockHead();
                                LJ2025GameManager.LockMovement = false;
                                _player.AttachHeadImmediately();
                                _refs.room3Light.gameObject.SetActive(false);
                                _refs.finalLight.gameObject.SetActive(true);
                                _refs.finalLight.enabled = true;
                                _refs.maintenanceDoor.Unlock();
                                _taskMs.EndTask();
                                _taskMs.StartTask("Follow the Light");
                            })
                            .Then(_ => _scheduler.When(() => _refs.guy3Door.IsOpen()))
                            .Then(_ => _scheduler.When(() => IsInRange("InMaintenance")))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _refs.maintenanceDoor.Close();
                                _refs.finalLight.gameObject.SetActive(false);
                            })
                            .Then(_ => _scheduler.Wait(1.4f))
                            .Then(_ =>
                            {
                                _refs.maintenanceLight.gameObject.SetActive(false);
                                _refs.ambiance.volume = 0;
                                _refs.wind.volume = 0;
                            })
                            .Then(_ => _scheduler.Wait(3))
                            .Then(_ =>
                            {
                                _refs.maintenanceLight.gameObject.SetActive(true);
                                _refs.maintenanceLight.enabled = false;
                                _refs.maintenanceLight.GetComponent<Light>().enabled = true;
                                _screenEffectMs.RestoreDefaults();
                                _refs.maintenanceDoor.SetOpenSpeed(0.10f);
                            })
                            .Then(_ => _scheduler.When(() => _refs.maintenanceDoor.IsOpen()))
                            .Then(_ =>
                            {
                                _refs.manager.gameObject.SetActive(true);
                                _player.DetachHeadLookAt(_refs.manager.transform.position + new Vector3(0, 1.4f, 0));
                                _player.LockHead();
                                LJ2025GameManager.LockMovement = true;
                                _refs.maintenanceLight.enabled = true;
                                _refs.spookSound.Play();
                                _scheduler.Wait(4)
                                    .Then(_ =>
                                    {
                                        float from = _refs.spookSound.volume;
                                        GameManager.GetMonoSystem<IAnimationMonoSystem>()
                                            .RequestAnimation(
                                                this,
                                                1.5f,
                                                t => _refs.spookSound.volume = Mathf.Lerp(from, 0.0f, t),
                                                () => { },
                                                true);
                                    });
                            })
                            .Then(_ => _scheduler.Wait(0.8f))
                            .Then(_ =>
                            {
                                _refs.manager.Next().Then(_ =>
                                {
                                    _refs.maintenanceDoor.Close();
                                });
                                Promise p = new();
                                Vector3 startPos = _player.transform.position;
                                Vector3 endPos = startPos + new Vector3(0, 0, 3);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                    this,
                                    1,
                                    t => _player.Teleport(new MathExt.Transform(
                                        Vector3.Lerp(startPos, endPos, t),
                                        _player.transform.rotation
                                    )),
                                    () =>
                                    {
                                        p.Resolve();
                                    },
                                    true);
                                return p;
                            })
                            .Then(_ =>
                            {
                                _refs.wind.volume = 1.5f;
                                _scheduler.Wait(4).Then(_ =>
                                {
                                    _refs.maintenanceLight.enabled = false;
                                });
                            })
                            .Then(_ => _dialogueMs.StartDialoguePromise("Confrontation"))
                            .Then(_ => _scheduler.Wait(3f))
                            .Then(_ => _screenEffectMs.Fadeout(1f))
                            .Then(_ => _screenEffectMs.FadeoutText("Later", 1f))
                            .Then(_ =>
                            {
                                _taskMs.EndTask();
                                _refs.tv.DisplayEnd();
                                GameManager.GetMonoSystem<IUIMonoSystem>().Show<EndView>(hideLastView: false);
                            })
                            .Then(_ => _screenEffectMs.FadeinText(1f))
                            .Then(_ => _screenEffectMs.Fadein(1f))
                            .Then(_ =>  _dialogueMs.StartDialoguePromise("End"))
                            .Then(_ => { Application.Quit(); });
                    
                    break;
                }
            }
        }

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime);
        }

        private bool _tmpTestStart = false;
        private void FixedUpdate()
        {
            if (Time.time > 1) Begin();
            _date = _date.Add(TimeSpan.FromSeconds(Time.fixedDeltaTime * _timeScale * _globalTimeScale));
        }

        public void Begin()
        {
            if (_tmpTestStart) return;
            _tmpTestStart = true;
            TriggerEvent("Begin", transform);
        }

        public void SetInRange(string id, bool state)
        {
            if (state) _inRange.Add(id);
            else _inRange.Remove(id);
        }

        public bool IsInRange(string id) => _inRange.Contains(id);
    }
}
