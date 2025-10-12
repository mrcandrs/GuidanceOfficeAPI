﻿using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
