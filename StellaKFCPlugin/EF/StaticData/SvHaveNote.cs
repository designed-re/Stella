using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// HAVE_NOTE definitions for BOOTH→infinite infection migration (asphyxia HAVE_NOTE).
/// </summary>
public partial class SvHaveNote
{
    public int Id { get; set; }

    public int NoteId { get; set; }

    public int Param { get; set; }
}