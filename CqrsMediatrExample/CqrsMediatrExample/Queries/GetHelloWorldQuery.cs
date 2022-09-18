using MediatR;
using System.Collections.Generic;

namespace CqrsMediatrExample.Queries
{
    public record GetHelloWorldQuery : IRequest<string>;
}
