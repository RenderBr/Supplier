namespace Supplier;
public class PlayerOperationState
{

    private bool _infChestAdd;
    public bool InfChestAdd
    {
        get => _infChestAdd;
        set
        {
            if (value) SetAllOperationStatesFalse();
            _infChestAdd = value;
        }
    }

    private bool _infChestAddBulk;
    public bool InfChestAddBulk
    {
        get => _infChestAddBulk;
        set
        {
            if (value) SetAllOperationStatesFalse();
            _infChestAddBulk = value;
        }
    }

    private bool _infChestDelBulk;
    public bool InfChestDelBulk
    {
        get => _infChestDelBulk;
        set
        {
            if (value) SetAllOperationStatesFalse();
            _infChestDelBulk = value;
        }
    }

    private bool _infChestDelete;
    public bool InfChestDelete
    {
        get => _infChestDelete;
        set
        {
            if (value) SetAllOperationStatesFalse();
            _infChestDelete = value;
        }
    }

    public void SetAllOperationStatesFalse()
    {
        _infChestAdd = false;
        _infChestAddBulk = false;
        _infChestDelete = false;
        _infChestDelBulk = false;
    }
}