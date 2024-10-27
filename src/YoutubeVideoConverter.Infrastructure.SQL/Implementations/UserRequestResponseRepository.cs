﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions;
using YoutubeVideoConverter.Infrastructure.SQL.Models;
using YoutubeVideoConverter.Infrastructure.SQL.Persistance;

namespace YoutubeVideoConverter.Infrastructure.SQL.Implementations
{
    public class UserRequestResponseRepository : BaseRepository<UserRequestResponse>, IUserRequestResponseRepository
    {
        public UserRequestResponseRepository(AppDbContext context) : base(context) { }
    }
}