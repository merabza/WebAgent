﻿using System.Collections.Generic;
using DbTools.Models;
using MessagingAbstractions;

namespace LibDatabasesMini.CommandRequests;

public sealed class GetDatabaseNamesCommandRequest : ICommand<IEnumerable<DatabaseInfoModel>>
{
    public static GetDatabaseNamesCommandRequest Create()
    {
        return new GetDatabaseNamesCommandRequest();
    }
}