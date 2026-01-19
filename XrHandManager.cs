using Godot;
using KitchenDesigner;
using System;

public enum HandSide { Left, Right }

public partial class XrHandManager : Node
{
    [Signal] public delegate void DominantHandButtonPressedEventHandler(XRController3D controller, string actionName);
    [Signal] public delegate void OtherHandButtonPressedEventHandler(XRController3D controller, string actionName);

    [Signal] public delegate void DominantHandButtonReleasedEventHandler(XRController3D controller, string actionName);
    [Signal] public delegate void OtherHandButtonReleasedEventHandler(XRController3D controller, string actionName);

    [Signal] public delegate void DominantRaycastStartedCollidingEventHandler(Node collider);

    [Signal] public delegate void DominantRaycastStoppedCollidingEventHandler();

    [Signal] public delegate void DominantHandChangedEventHandler(XRController3D oldSide, XRController3D newSide);

    [Export] public HandSide DominantHand { get; private set; } = HandSide.Right;

    [ExportGroup("Controllers")]
    [Export] public XRController3D LeftController;
    [Export] public XRController3D RightController;

    [ExportGroup("Tips & Pointers")]
    [Export] public Marker3D LeftTip;
    [Export] public Marker3D RightTip;
    [Export] public Node3D LeftPointer;
    [Export] public Node3D RightPointer;

    [ExportGroup("UI")]
    [Export] public Node3D HandMenu;

    private Node _lastCollider = null;

    private DesignerEvents _designerEvents;
    public override void _Ready()
    {
        LeftController.ButtonPressed += (name) => OnButtonPressed(LeftController, HandSide.Left, name);
        LeftController.ButtonReleased += (name) => OnButtonReleased(LeftController, HandSide.Left, name);
        RightController.ButtonPressed += (name) => OnButtonPressed(RightController, HandSide.Right, name);
        RightController.ButtonReleased += (name) => OnButtonReleased(RightController, HandSide.Right, name);

        _designerEvents = GetNode<DesignerEvents>("/root/DesignerEvents");
        _designerEvents.RequestDominantHandChange += (isRight) =>
        {
            SetDominantHand(isRight ? HandSide.Right : HandSide.Left);
        };

        UpdateHandSetup();
    }

    public override void _Process(double delta)
    {
        UpdateRaycastCollision();
    }

    public void SetDominantHand(HandSide side)
    {
        if (side == DominantHand) return;
        DominantHand = side;
        UpdateHandSetup();
        EmitSignal(SignalName.DominantHandChanged, GetOtherController(), GetActiveController());
    }

    private void UpdateHandSetup()
    {
        bool isRight = DominantHand == HandSide.Right;

        RightPointer.Visible = isRight;
        LeftPointer.Visible = !isRight;

        RightPointer.GetNode<RayCast3D>("RayCast").Enabled = isRight;
        LeftPointer.GetNode<RayCast3D>("RayCast").Enabled = !isRight;

        // Přeparkování Menu na nedominantní ruku
        if (HandMenu != null)
        {
            Node3D menuParent = isRight ? LeftController : RightController;
            if (HandMenu.GetParent() != menuParent)
            {
                Callable.From(() =>
                {
                    HandMenu.GetParent()?.RemoveChild(HandMenu);
                    menuParent.AddChild(HandMenu);
                    HandMenu.Position = new(0, -0.09f, -0.32f);
                    HandMenu.Rotation = new(Single.DegreesToRadians(-84), 0, 0);
                }).CallDeferred();

            }
        }
    }

    private void UpdateRaycastCollision()
    {
        RayCast3D activeRay = GetActiveRayCast();
        Node currentCollider = null;

        if (activeRay.IsColliding())
        {
            currentCollider = activeRay.GetCollider() as Node;
        }

        // Detekce změny (začátek / konec / změna objektu)
        if (currentCollider != _lastCollider)
        {
            if (_lastCollider != null)
            {
                EmitSignal(SignalName.DominantRaycastStoppedColliding);
            }

            if (currentCollider != null)
            {
                EmitSignal(SignalName.DominantRaycastStartedColliding, currentCollider);
            }

            _lastCollider = currentCollider;
        }
    }

    private void OnButtonPressed(XRController3D controller, HandSide side, string actionName)
    {
        GD.Print($"Tlačítko '{actionName}' stisknuto na ruce '{side}'");
        if (side == DominantHand)
        {
            EmitSignal(SignalName.DominantHandButtonPressed, controller, actionName);
        }
        else
        {
            EmitSignal(SignalName.OtherHandButtonPressed, controller, actionName);

            if (actionName == "grip_click")
            {
                HandMenu.Visible = true;
            }
        }
    }

    private void OnButtonReleased(XRController3D controller, HandSide side, string actionName)
    {
        GD.Print($"Tlačítko '{actionName}' uvolněno na ruce '{side}'");
        if (side == DominantHand)
        {
            EmitSignal(SignalName.DominantHandButtonReleased, controller, actionName);
        }
        else
        {
            EmitSignal(SignalName.OtherHandButtonReleased, controller, actionName);

            if (actionName == "grip_click")
            {
                HandMenu.Visible = false;
            }
        }
    }

    public void VibrateController(HandSide side, float amplitude = 0.5f, float duration = 0.1f, float frequency = 1.0f)
    {
        XRController3D controller = (side == HandSide.Right) ? RightController : LeftController;

        if (controller != null)
        {
            controller.TriggerHapticPulse("haptic", frequency, amplitude, duration, 0);
        }
    }

    public void VibrateDominantHand(float amplitude = 0.5f, float duration = 0.1f)
    {
        VibrateController(DominantHand, amplitude, duration);
    }

    public void VibrateOtherHand(float amplitude = 0.5f, float duration = 0.1f)
    {
        VibrateController(DominantHand == HandSide.Right ? HandSide.Left : HandSide.Right, amplitude, duration);
    }

    public void SetPointerLayerEnabled(int layer, bool enabled)
    {
        var pointer = GetDominantPointer();
        if (pointer == null)
        {
            GD.PrintErr("Nedominantní ukazatel ruky není přiřazen!");
            return;

        }

        Variant currentMaskVar = pointer.Get("collision_mask");

        if (currentMaskVar.VariantType == Variant.Type.Nil)
        {
            GD.PrintErr($"Pointer {pointer.Name} nemá vlastnost 'collision_mask'!");
            return;
        }

        uint currentMask = currentMaskVar.As<uint>();

        if (enabled)
        {
            currentMask |= (uint)(1 << (layer - 1));
        }
        else
        {
            currentMask &= ~(uint)(1 << (layer - 1));
        }

        pointer.Set("collision_mask", currentMask);
    }

    public XRController3D GetActiveController() => DominantHand == HandSide.Right ? RightController : LeftController;
    public XRController3D GetOtherController() => DominantHand == HandSide.Right ? LeftController : RightController;
    public XRController3D GetController(HandSide side) => side == HandSide.Right ? RightController : LeftController;

    public Marker3D GetActiveTip() => DominantHand == HandSide.Right ? RightTip : LeftTip;
    public Marker3D GetOtherTip() => DominantHand == HandSide.Right ? LeftTip : RightTip;
    public Marker3D GetTip(HandSide side) => side == HandSide.Right ? RightTip : LeftTip;

    public RayCast3D GetActiveRayCast() => GetRayCast(DominantHand);
    public RayCast3D GetOtherRayCast() => GetRayCast(DominantHand == HandSide.Right ? HandSide.Left : HandSide.Right);
    private RayCast3D GetRayCast(HandSide side) => (side == HandSide.Right ? RightPointer : LeftPointer).GetNode<RayCast3D>("RayCast");

    public Node3D GetDominantPointer() => GetPointer(DominantHand);
    public Node3D GetOtherPointer() => GetPointer(DominantHand == HandSide.Right ? HandSide.Left : HandSide.Right);
    public Node3D GetPointer(HandSide side) => side == HandSide.Right ? RightPointer : LeftPointer;
}