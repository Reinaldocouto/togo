using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Togo.Infrastructure.Persistence;

#nullable disable

namespace Togo.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260630120000_AddAttendanceClinicId")]
public partial class AddAttendanceClinicId
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.22");
#pragma warning restore 612, 618
    }
}
