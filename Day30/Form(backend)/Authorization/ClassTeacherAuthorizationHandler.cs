
using Form.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Form.Authorization
{
    public class ClassTeacherAuthorizationHandler(AppDbContext db) : AuthorizationHandler<ClassTeacherRequirement>
    {

        protected override async Task HandleRequirementAsync(
       AuthorizationHandlerContext context, ClassTeacherRequirement requirement)
        {
            // Admins outrank every class teacher by design — always pass.
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }


            // The classroom ID arrives one of two ways depending on the endpoint:
            Guid? classRoomId = context.Resource switch
            {
                // Declarative [Authorize(Policy = "...")] on a route like
                // "class/{classRoomId:guid}" — the framework hands the handler
                // the whole HttpContext, so we pull the ID out of route data.
                HttpContext http when http.GetRouteValue("classRoomId") is string s
                    && Guid.TryParse(s, out var fromRoute) => fromRoute,

                // Imperative IAuthorizationService.AuthorizeAsync(User, someGuid, "...")
                // call, used when the ID is in the request body instead of the URL
                // (this is the path AttendanceController.Mark uses).
                Guid explicitId => explicitId,

                _ => null
            };
            if (classRoomId is null) return;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return;

            var isClassTeacher = await db.ClassRooms
                .AsNoTracking()
                .AnyAsync(c => c.Id == classRoomId && c.ClassTeacherUserId == userId);

            if (isClassTeacher) context.Succeed(requirement);
            // No context.Fail() here on purpose — leaving it unset (rather than
            // explicitly failed) means a future SECOND handler for this same
            // requirement could still succeed independently, if you ever add one.
        }
    }
}