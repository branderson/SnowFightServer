namespace Networking.Data
{
    public enum Packets
    {
        None = 0,
        String = 1,
        Login = 2,
        AckLogin = 3,
        JoinTeam = 4,
        AckJoinTeam = 5,
        DestroyObject = 6,
        PlayerUpdate = 7,
        PlayerSync = 8,
        SpawnSnowball = 9,
        SnowballSync = 10,
    }
}