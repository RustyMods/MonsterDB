using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonsterDB;

[HarmonyPatch(typeof(Hud), nameof(Hud.UpdateMount))]
public static class Hud_UpdateMount
{
    private static bool Prefix(Hud __instance, Player player, float dt)
    {
        Saddle? doodadController = player.GetDoodadController() as Saddle;
        if (doodadController == null) return true;

        Character character = doodadController.GetCharacter();
        __instance.m_mountPanel.SetActive(true);
        __instance.m_mountIcon.overrideSprite = doodadController.m_mountIcon;
        __instance.m_mountHealthBarSlow.SetValue(character.GetHealthPercentage());
        __instance.m_mountHealthBarFast.SetValue(character.GetHealthPercentage());
        __instance.m_mountHealthText.text = Mathf.CeilToInt(character.GetHealth()).ToFastString();
        float stamina = doodadController.GetStamina();
        float maxStamina = doodadController.GetMaxStamina();
        __instance.m_mountStaminaBar.SetValue(stamina / maxStamina);
        __instance.m_mountStaminaText.text = Mathf.CeilToInt(stamina).ToFastString();
        __instance.m_mountNameText.text = $"{character.GetHoverName()} ( {Localization.instance.Localize(doodadController.GetTameable().GetStatusString())} )";
        
        return false;
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.IsRiding))]
public static class Player_IsRiding
{
    private static void Postfix(Player __instance, ref bool __result)
    {
        __result |= __instance.m_doodadController != null && 
                    __instance.m_doodadController.IsValid() && 
                    __instance.m_doodadController is Saddle;
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
public static class Player_SetControls_SaddlePatch
{
    private static bool Prefix(Player __instance, 
        Vector3 movedir, 
        bool attack, 
        bool secondaryAttack,
        bool block, 
        bool jump, 
        bool run, 
        bool autoRun)
    {
        if (__instance.m_doodadController is not Saddle saddle || !saddle.IsValid()) return true;
        saddle.ApplyCustomControls(movedir, __instance.m_lookDir, run, autoRun, block, attack, secondaryAttack, jump);
        return false;
    }
}

public class Saddle : MonoBehaviour, Interactable, Hoverable, IDoodadController
{
    public string m_hoverText = "";
    public float m_maxUseRange = 10f;
    public Transform? m_attachPoint;
    public Vector3 m_attachOffset;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GameObject m_attachParent;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Vector3 m_detachOffset = new(0.0f, 0.5f, 0.0f);
    public string m_attachAnimation = "attach_chair";
    public float m_maxStamina = 100f;
    public float m_runStaminaDrain = 10f;
    public float m_swimStaminaDrain = 10f;
    public float m_staminaRegen = 10f;
    public float m_staminaRegenHungry = 10f;
    public EffectList m_drownEffects = new();
    public Sprite? m_mountIcon;
    public Vector3 m_controlDir;
    public Sadle.Speed m_speed;
    public float m_rideSkill;
    public float m_staminaRegenTimer;
    public float m_drownDamageTimer;
    public float m_raiseSkillTimer;
    public Character m_character = null!;
    public ZNetView m_nview = null!;
    public Tameable m_tambable = null!;
    public BaseAI m_baseAI = null!;
    public bool m_haveValidUser;

    public const string rideKey = "JoyLTrigger";
    private const string removeKey = "JoyButtonB";

    public void Awake()
    {
        m_character = GetComponent<Character>();
        m_nview = GetComponent<ZNetView>();
        m_tambable = GetComponent<Tameable>();
        m_baseAI = GetComponent<BaseAI>();
        
        m_nview.Register("RequestControl", new Action<long, long>(RPC_RequestControl));
        m_nview.Register("ReleaseControl", new Action<long, long>(RPC_ReleaseControl));
        m_nview.Register("RequestRespons", new Action<long, bool>(RPC_RequestRespons));
        m_nview.Register("RemoveSaddle", new Action<long, Vector3>(RPC_RemoveSaddle));
        m_nview.Register("Controls", new Action<long, Vector3, int, float>(RPC_Controls));
        
        m_nview.Register("AddSaddle", RPC_AddSaddle);
    }

    public void Start()
    {
        GameObject attach = new GameObject("attach");
        attach.transform.SetParent(m_attachParent != null ? m_attachParent.transform : transform);
        attach.transform.localPosition = m_attachOffset;
        attach.transform.rotation = Quaternion.identity;
        m_attachPoint = attach.transform;
    }

    public void Restart()
    {
        if (m_attachPoint != null)
        {
            m_attachPoint.SetParent(m_attachParent != null ? m_attachParent.transform : transform);
            m_attachPoint.localPosition = m_attachOffset;
            m_attachPoint.rotation = Quaternion.identity;
            ValidateAttachRotation();
        }
    }

    public bool IsValid()
    {
        return this && m_attachPoint != null;
    }
    
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.UseItem))]
    public static class Tameable_UseItem
    {
        private static bool Prefix(Tameable __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__instance.TryGetComponent(out Saddle component)) return true;
            __result = component.UseItem(user, item);
            return false;
        }
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        if (!m_nview.IsValid() || m_tambable.m_saddleItem == null || !m_tambable.IsTamed() ||
            item.m_shared.m_name != m_tambable.m_saddleItem.m_itemData.m_shared.m_name) return false;

        if (HaveSaddle())
        {
            user.Message(MessageHud.MessageType.Center, m_tambable.GetHoverName() + " $hud_saddle_already");
        }
        else
        {
            m_nview.InvokeRPC("AddSaddle");
            user.GetInventory().RemoveOneItem(item);
            user.Message(MessageHud.MessageType.Center, m_tambable.GetHoverName() + " $hud_saddle_ready");
        }

        return true;
    }

    public void FixedUpdate()
    {
        if (!m_nview.IsValid()) return;
        CalculateHaveValidUser();
        
        if (!m_character.IsTamed()) return;
        
        if (IsLocalUser()) UpdateRidingSkill(Time.fixedDeltaTime);
        
        if (!m_nview.IsOwner()) return;
        
        float fixedDeltaTime = Time.fixedDeltaTime;
        UpdateStamina(fixedDeltaTime);
        UpdateDrown(fixedDeltaTime);
    }

    public void UpdateDrown(float dt)
    {
        if (!m_character.IsSwimming() || m_character.IsOnGround() || HaveStamina())
            return;
        m_drownDamageTimer += dt;
        if (m_drownDamageTimer <= 1.0)
            return;
        m_drownDamageTimer = 0.0f;
        float num = Mathf.Ceil(m_character.GetMaxHealth() / 20f);
        m_character.Damage(new HitData
        {
            m_damage =
            {
                m_damage = num
            },
            m_point = m_character.GetCenterPoint(),
            m_dir = Vector3.down,
            m_pushForce = 10f,
            m_hitType = HitData.HitType.Drowning
        });
        m_drownEffects.Create(transform.position with
        {
            y = m_character.GetLiquidLevel()
        }, transform.rotation);
    }
    
    [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.UpdateAI))]
    public static class BaseAI_UpdateAI
    {
        private static bool Prefix(BaseAI __instance, float dt, ref bool __result)
        {
            if (!__instance.TryGetComponent(out Saddle component)) return true;
            if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return true;
            if (!__instance.m_character.IsTamed()) return true;

            if (component.UpdateRiding(dt))
            {
                __instance.UpdateTakeoffLanding(dt);
                if (__instance.m_jumpInterval > 0.0)
                {
                    __instance.m_jumpTimer += dt;
                }

                if (__instance.m_randomMoveUpdateTimer > 0.0)
                {
                    __instance.m_randomMoveUpdateTimer -= dt;
                }

                __instance.UpdateRegeneration(dt);
                __instance.m_timeSinceHurt += dt;

                __result = false;
                return false;
            }

            return true;
        }
    }

    public bool UpdateRiding(float dt)
    {
        if (!m_character.IsTamed() || !HaveValidUser() || m_speed == Sadle.Speed.Stop || m_controlDir.magnitude == 0.0)
            return false;
        
        if (m_speed == Sadle.Speed.Walk || m_speed == Sadle.Speed.Run)
        {
            if (m_speed == Sadle.Speed.Run && !HaveStamina())
            {
                m_speed = Sadle.Speed.Walk;
            }
            m_baseAI.MoveTowards(m_controlDir, m_speed == Sadle.Speed.Run);
            float num = Mathf.Lerp(1f, 0.5f, GetRiderSkill());
            if (m_character.IsSwimming() || m_character.IsFlying())
            {
                UseStamina(m_swimStaminaDrain * num * dt);
            }
            else if (m_speed == Sadle.Speed.Run)
            {
                UseStamina(m_runStaminaDrain * num * dt);
            }
        }
        else if (m_speed == Sadle.Speed.Turn)
        {
            m_baseAI.StopMoving();
            m_character.SetRun(false);
            m_baseAI.LookTowards(m_controlDir);
        }

        m_baseAI.ResetRandomMovement();
        return true;
    }

    public bool HasSaddleItem() => m_tambable.m_saddleItem != null;

    public bool HaveSaddle()
    {
        return m_nview.IsValid() && m_nview.GetZDO().GetBool(ZDOVars.s_haveSaddleHash);
    }

    public void RPC_AddSaddle(long sender)
    {
        if (!m_nview.IsOwner() || HaveSaddle()) return;
        m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, true);
        m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", true);
    }

    public bool DropSaddle(Vector3 userPoint)
    {
        if (!HaveSaddle())
        {
            return false;
        }
        m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, false);
        m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", false);
        SpawnSaddle(userPoint - transform.position);
        return true;
    }

    public void SpawnSaddle(Vector3 flyDirection)
    {
        Rigidbody? component =
            Instantiate(m_tambable.m_saddleItem.gameObject, transform.TransformPoint(m_tambable.m_dropSaddleOffset),
                Quaternion.identity).GetComponent<Rigidbody>();
        if (!component) return;
        Vector3 up = Vector3.up;
        if (flyDirection.magnitude > 0.10000000149011612)
        {
            flyDirection.y = 0.0f;
            flyDirection.Normalize();
            up += flyDirection;
        }

        component.AddForce(up * m_tambable.m_dropItemVel, ForceMode.VelocityChange);
    }
    
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    public static class Tameable_GetHoverText
    {
        private static void Postfix(Tameable __instance, ref string __result)
        {
            if (!__instance.TryGetComponent(out Saddle component)) return;
            if (!__instance.IsTamed() || (component.HasSaddleItem() && !component.HaveSaddle())) return;
            __result += component.GetHoverText();
        }
    }

    public string GetHoverText()
    {
        if (!m_nview.IsValid()) return "";
        StringBuilder sb = new();
        bool usingGamepad = ZInput.IsGamepadActive();
        
        if (usingGamepad)
        {
            sb.Append($"\n[<color=yellow><b>{ZInput.instance.GetBoundKeyString(rideKey)} + $KEY_Use</b></color>] $hud_ride");
            if (HasSaddleItem()) sb.Append($"\n[<color=yellow><b>{ZInput.instance.GetBoundKeyString(removeKey)} + $KEY_Use</b></color>] $hud_saddle_remove");
        }
        else
        {
            sb.Append("\n[<color=yellow><b>$button_lalt + $KEY_Use</b></color>] $hud_ride");
            if (HasSaddleItem()) sb.Append("\n[<color=yellow><b>$button_lctrl + $KEY_Use</b></color>] $hud_saddle_remove");
        }
        
        return Localization.instance.Localize(sb.ToString());
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(m_hoverText);
    }
    
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
    public static class Tameable_Interact
    {
        private static bool Prefix(Tameable __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            if (!__instance.TryGetComponent(out Saddle component)) return true;

            if (component.HasSaddleItem() && !component.HaveSaddle()) return true;

            if (alt || hold) return true;

            if (ZInput.GetKey(KeyCode.LeftAlt) || ZInput.GetKeyDown(KeyCode.LeftAlt) || ZInput.GetButton(rideKey))
            {
                __result = component.Interact(user, false, false);
                return false;
            }

            if (ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKeyDown(KeyCode.LeftControl) || ZInput.GetButton(removeKey))
            {
                __result = component.Interact(user, false, true);
                return false;
            }
            
            return true;
        }
    }

    public bool Interact(Humanoid character, bool repeat, bool alt)
    {
        if (repeat || !m_nview.IsValid() || !InUseDistance(character) || !m_character.IsTamed()) return false;
        Player? player = character as Player;
        if (player == null) return false;
        
        if (alt && HasSaddleItem())
        {
            m_nview.InvokeRPC("RemoveSaddle", character.transform.position);
            return true;
        }

        m_nview.InvokeRPC("RequestControl", player.GetZDOID().UserID);
        return false;
    }

    public Character GetCharacter()
    {
        return m_character;
    }

    public Tameable GetTameable()
    {
        return m_tambable;
    }

    public void ApplyCustomControls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block, bool attack, bool attackSecondary, bool jump)
    {
        if (Player.m_localPlayer == null) return;
        
        if (jump)
        {
            if (m_character.GetBaseAI().m_randomFly || m_character.CanToggleFly())
            {
                if (m_character.IsFlying())
                {
                    m_character.Land();
                }
                else
                {
                    m_character.TakeOff();
                }
            }
            else
            {
                m_character.Jump();
            }
        }
        
        ApplyControlls(moveDir, lookDir, run, autoRun, block);
        
        if (m_character.GetBaseAI() is MonsterAI monsterAI && m_character is Humanoid humanoid)
        {
            if (attack || attackSecondary)
            {
                List<ItemDrop.ItemData> items = humanoid.GetAvailableAttacks();
                if (items.Count > 0)
                {
                    ItemDrop.ItemData? randomItem = items[UnityEngine.Random.Range(0, items.Count)];
                    humanoid.EquipItem(randomItem);
                }
                monsterAI.DoAttack(null, false);
            }
        }
    }
    
    public void ApplyControlls(
        Vector3 moveDir,
        Vector3 lookDir,
        bool run,
        bool autoRun,
        bool block)
    {
        if (Player.m_localPlayer == null) return;

        float rideSkill = Player.m_localPlayer
            .GetSkills()
            .GetSkillFactor(Skills.SkillType.Ride);

        Sadle.Speed speed = Sadle.Speed.NoChange;
        Vector3 desiredDirection = Vector3.zero;

        bool hasMovementIntent =
            run ||
            block ||
            moveDir.z > 0.5f;

        if (hasMovementIntent)
        {
            desiredDirection = lookDir;
            if (!m_character.IsFlying()) desiredDirection.y = 0f;

            if (desiredDirection.sqrMagnitude > 0f)
                desiredDirection.Normalize();
        }

        if (run)
        {
            speed = Sadle.Speed.Run;
        }
        else if (moveDir.z > 0.5f)
        {
            speed = Sadle.Speed.Walk;
        }
        else if (moveDir.z < -0.5f)
        {
            speed = Sadle.Speed.Stop;
        }
        else if (block)
        {
            speed = Sadle.Speed.Turn;
        }

        m_nview.InvokeRPC("Controls", desiredDirection, (int)speed, rideSkill);
    }

    public void RPC_Controls(long sender, Vector3 rideDir, int rideSpeed, float skill)
    {
        if (!m_nview.IsOwner()) return;
        m_rideSkill = skill;
        if (rideDir != Vector3.zero)
        {
            m_controlDir = rideDir;
        }
        switch (rideSpeed)
        {
            case 3:
                if (m_speed == Sadle.Speed.Walk || m_speed == Sadle.Speed.Run) return;
                break;
            case 4:
                if (m_speed != Sadle.Speed.Turn) return;
                m_speed = Sadle.Speed.Stop;
                return;
        }

        m_speed = (Sadle.Speed)rideSpeed;
    }

    public void UpdateRidingSkill(float dt)
    {
        m_raiseSkillTimer += dt;
        if (m_raiseSkillTimer <= 1.0) return;
        m_raiseSkillTimer = 0.0f;
        if (m_speed != Sadle.Speed.Run) return;
        Player.m_localPlayer.RaiseSkill(Skills.SkillType.Ride);
    }

    public void ResetControlls()
    {
        m_controlDir = Vector3.zero;
        m_speed = Sadle.Speed.Stop;
        m_rideSkill = 0.0f;
    }

    public Component GetControlledComponent()
    {
        return m_character;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void RPC_RemoveSaddle(long sender, Vector3 userPoint)
    {
        if (!m_nview.IsOwner() || HaveValidUser()) return;
        DropSaddle(userPoint);
    }

    public void RPC_RequestControl(long sender, long playerID)
    {
        if (!m_nview.IsOwner()) return;
        CalculateHaveValidUser();
        if (GetUser() == playerID || !HaveValidUser())
        {
            ValidateAttachRotation();
            m_nview.GetZDO().Set(ZDOVars.s_user, playerID);
            ResetControlls();
            m_nview.InvokeRPC(sender, "RequestRespons", true);
            m_nview.GetZDO().SetOwner(sender);
        }
        else
        {
            m_nview.InvokeRPC(sender, "RequestRespons", false);
        }
    }

    // private GameObject? line;
    // public void StartLines(Player player)
    // {
    //     if (m_attachPoint == null) return;
    //     try
    //     {
    //         var leftHandAttach = player.m_visEquipment.m_leftHand;
    //         var rightHandAttach = player.m_visEquipment.m_rightHand;
    //         var head = m_character.m_head;
    //         
    //         if (leftHandAttach == null || rightHandAttach == null || head == null) return;
    //
    //         var vfx_harpoon = PrefabManager.GetPrefab("vfx_Harpooned");
    //         var vfx_renderer = vfx_harpoon.GetComponent<LineRenderer>();
    //
    //         line = new GameObject("line");
    //         line.transform.SetParent(m_attachPoint);
    //         LineRenderer? lineRenderer = line.AddComponent<LineRenderer>();
    //         lineRenderer.material = vfx_renderer.material;
    //         lineRenderer.startWidth = 0.1f;
    //         lineRenderer.endWidth = 0.1f;
    //         lineRenderer.positionCount = 3;
    //         lineRenderer.SetPosition(0, leftHandAttach.position);
    //         lineRenderer.SetPosition(1, head.position);
    //         lineRenderer.SetPosition(2, rightHandAttach.position);
    //     }
    //     catch (Exception ex)
    //     {
    //         MonsterDBPlugin.LogError(ex.Message);
    //         MonsterDBPlugin.LogWarning(ex.StackTrace);
    //     }
    // }
    //
    // public void StopLines()
    // {
    //     if (line == null) return;
    //     Destroy(line);
    //     line = null;
    // }

    public void ValidateAttachRotation()
    {
        if (m_attachPoint == null) return;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            return;

        forward.Normalize();

        m_attachPoint.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    public bool HaveValidUser()
    {
        return m_haveValidUser;
    }

    public void CalculateHaveValidUser()
    {
        m_haveValidUser = false;
        if (m_attachPoint == null) return;
        long user = GetUser();
        if (user == 0L) return;
        List<ZDO>? zdos = ZNet.instance.GetAllCharacterZDOS();
        for (int i = 0; i < zdos.Count; ++i)
        {
            ZDO? zdo = zdos[i];
            if (zdo.m_uid.UserID == user)
            {
                m_haveValidUser = Vector3.Distance(zdo.GetPosition(), transform.position) < m_maxUseRange;

                break;
            }
        }
    }

    public void RPC_ReleaseControl(long sender, long playerID)
    {
        if (!m_nview.IsOwner() || GetUser() != playerID) return;
        m_nview.GetZDO().Set(ZDOVars.s_user, 0L);
        ResetControlls();
    }

    public void RPC_RequestRespons(long sender, bool granted)
    {
        if (!Player.m_localPlayer) return;
        if (granted && m_attachPoint != null)
        {
            Player.m_localPlayer.StartDoodadControl(this);
            Player.m_localPlayer.AttachStart(m_attachPoint, gameObject, false, false, false, m_attachAnimation, m_detachOffset);
            // StartLines(Player.m_localPlayer);
        }
        else
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse");
        }
    }

    public void OnUseStop(Player player)
    {
        if (!m_nview.IsValid()) return;
        m_nview.InvokeRPC("ReleaseControl", player.GetZDOID().UserID);
        if (m_attachPoint == null) return;
        player.AttachStop();
        // StopLines();
    }

    public bool IsLocalUser()
    {
        if (!Player.m_localPlayer) return false;
        long user = GetUser();
        return user != 0L && user == Player.m_localPlayer.GetZDOID().UserID;
    }

    public long GetUser()
    {
        return m_nview == null || !m_nview.IsValid() ? 0L : m_nview.GetZDO().GetLong(ZDOVars.s_user);
    }

    public bool InUseDistance(Humanoid human)
    {
        if (m_attachPoint == null) return false;
        return Vector3.Distance(human.transform.position, m_attachPoint.position) < (double)m_maxUseRange;
    }

    public void UseStamina(float v)
    {
        if (v == 0.0 || !m_nview.IsValid() || !m_nview.IsOwner()) return;
        float stamina = GetStamina() - v;
        if (stamina < 0.0) stamina = 0.0f;
        SetStamina(stamina);
        m_staminaRegenTimer = 1f;
    }

    public bool HaveStamina(float amount = 0.0f)
    {
        return m_nview.IsValid() && GetStamina() > (double)amount;
    }

    public float GetStamina()
    {
        return m_nview == null || m_nview.GetZDO() == null
            ? 0.0f
            : m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, GetMaxStamina());
    }

    public void SetStamina(float stamina)
    {
        m_nview.GetZDO().Set(ZDOVars.s_stamina, stamina);
    }

    public float GetMaxStamina()
    {
        return m_maxStamina;
    }

    public void UpdateStamina(float dt)
    {
        m_staminaRegenTimer -= dt;

        if (m_staminaRegenTimer > 0f)
            return;

        if (m_character.InAttack() || m_character.IsSwimming())
            return;

        float currentStamina = GetStamina();
        float maxStamina = GetMaxStamina();

        if (currentStamina >= maxStamina)
            return;

        float baseRegenRate = m_tambable.IsHungry()
            ? m_staminaRegenHungry
            : m_staminaRegen;

        // Regen scales up as stamina gets lower
        float missingFraction = 1f - currentStamina / maxStamina;
        float regenRate = baseRegenRate + missingFraction * baseRegenRate;

        float newStamina = currentStamina + regenRate * dt;

        SetStamina(Mathf.Min(newStamina, maxStamina));
    }

    public float GetRiderSkill()
    {
        return m_rideSkill;
    }
}