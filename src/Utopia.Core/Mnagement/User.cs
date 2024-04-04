// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace Utopia.Core.Mnagement;

/// <summary>
/// Information of an user with private information like password.
/// </summary>
public class User
{
    /// <summary>
    /// The email of the account.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string PasswordSha256 { get; set; } = string.Empty;

    /// <summary>
    /// The salt of the password.
    /// </summary>
    public string Salt { get; set; } = string.Empty;
}
