public interface MovementHandler
{
    void Move(PlayerCharacterController player);

    bool ShouldGravityApply(PlayerCharacterController player)
    {
        return true;
    }

    bool ShouldHoverApply(PlayerCharacterController player)
    {
        return true;
    }
}