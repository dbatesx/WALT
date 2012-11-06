using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WALT.BLL
{
    /// <summary>
    /// 
    /// </summary>
    public class Logger
    {
        #pragma warning disable 1591
        public enum Type_E { INFO, WARNING, ERROR };
        #pragma warning restore 1591

        DAL.Mediator _dalMediator;
        DTO.Profile _profile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="mediator"></param>
        public Logger(DTO.Profile profile, DAL.Mediator mediator)
        {
            _dalMediator = mediator;
            _profile = profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogInfo(string item)
        {
            Debug.WriteLine(_profile.DisplayName + " - INFO: " + item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogError(string item)
        {
            Debug.WriteLine(_profile.DisplayName + " - ERROR: " + item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogWarning(string item)
        {
            Debug.WriteLine(_profile.DisplayName + " - WARNING: " + item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="comment"></param>
        public void LogComment(DTO.Object dto, string comment)
        {
            _dalMediator.GetProfileProcessor().LogComment(_profile, dto, Type_E.INFO.ToString(), comment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="type"></param>
        /// <param name="comment"></param>
        public void LogComment(DTO.Object dto, Type_E type, string comment)
        {
            _dalMediator.GetProfileProcessor().LogComment(_profile, dto, type.ToString(), comment);
        }
    }
}
