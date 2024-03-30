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
/// 代表一个用户
/// </summary>
public class User
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long UniqueID { get; set; }

    /// <summary>
    /// The password of the user
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// The salt of the password
    /// </summary>
    public string Salt { get; set; }

    /// <summary>
    /// 用户的邮箱，可以用于gravatar等处
    /// </summary>
    public string Email { get; set; }
}
