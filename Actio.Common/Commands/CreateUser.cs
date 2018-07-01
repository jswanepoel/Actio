namespace Actio.Common.Commands
{
    public class CreateUser : ICommand
    {
        string Email { get; }
        string Password { get; }
        string Name { get; }
    }
}