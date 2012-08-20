/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

namespace FluorineFx.Scheduling
{
    ///<summary>
    /// Misfire instructions.
    ///</summary>
    public struct MisfireInstruction
    {
        /// <summary>
        /// Instruction not set (yet).
        /// </summary>
        public const int InstructionNotSet = 0;

        /// <summary>
        /// Use smart policy.
        /// </summary>
        public const int SmartPolicy = 0;

        /// <summary> 
        /// Instructs the <see cref="IScheduler" /> that upon a mis-fire
        /// situation, the <see cref="SimpleTrigger" /> wants to be fired
        /// now by <see cref="IScheduler" />.
        /// <p>
        /// <i>NOTE:</i> This instruction should typically only be used for
        /// 'one-shot' (non-repeating) Triggers. If it is used on a trigger with a
        /// repeat count > 0 then it is equivalent to the instruction 
        /// <see cref="RescheduleNowWithRemainingRepeatCount " />.
        /// </p>
        /// </summary>		
        public const int FireNow = 1;
        /// <summary>
        /// Instructs the <see cref="IScheduler" /> that upon a mis-fire
        /// situation, the <see cref="SimpleTrigger" /> wants to be
        /// re-scheduled to 'now' (even if the associated <see cref="ICalendar" />
        /// excludes 'now') with the repeat count left as-is.   This does obey the
        /// <see cref="Trigger" /> end-time however, so if 'now' is after the
        /// end-time the <code>Trigger</code> will not fire again.
        /// <p>
        /// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
        /// the start-time and repeat-count that it was originally setup with (this
        /// is only an issue if you for some reason wanted to be able to tell what
        /// the original values were at some later time).
        /// </p>
        /// 
        /// <p>
        /// <i>NOTE:</i> This instruction could cause the <see cref="Trigger" />
        /// to go to the 'COMPLETE' state after firing 'now', if all the
        /// repeat-fire-times where missed.
        /// </p>
        /// </summary>
        public const int RescheduleNowWithExistingRepeatCount = 2;
        /// <summary>
        /// Instructs the <see cref="IScheduler" /> that upon a mis-fire
        /// situation, the <see cref="SimpleTrigger" /> wants to be
        /// re-scheduled to 'now' (even if the associated <see cref="ICalendar" />
        /// excludes 'now') with the repeat count set to what it would be, if it had
        /// not missed any firings. This does obey the <see cref="Trigger" /> end-time 
        /// however, so if 'now' is after the end-time the <see cref="Trigger" /> will 
        /// not fire again.
        /// 
        /// <p>
        /// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
        /// the start-time and repeat-count that it was originally setup with (this
        /// is only an issue if you for some reason wanted to be able to tell what
        /// the original values were at some later time).
        /// </p>
        /// 
        /// <p>
        /// <i>NOTE:</i> This instruction could cause the <see cref="Trigger" />
        /// to go to the 'COMPLETE' state after firing 'now', if all the
        /// repeat-fire-times where missed.
        /// </p>
        /// </summary>
        public const int RescheduleNowWithRemainingRepeatCount = 3;

        /// <summary> 
        /// Instructs the <see cref="IScheduler" /> that upon a mis-fire
        /// situation, the <see cref="SimpleTrigger" /> wants to be
        /// re-scheduled to the next scheduled time after 'now' - taking into
        /// account any associated <see cref="ICalendar" />, and with the
        /// repeat count set to what it would be, if it had not missed any firings.
        /// </summary>
        /// <remarks>
        /// <i>NOTE/WARNING:</i> This instruction could cause the <see cref="Trigger" />
        /// to go directly to the 'COMPLETE' state if all fire-times where missed.
        /// </remarks>
        public const int RescheduleNextWithRemainingCount = 4;

        /// <summary>
        /// Instructs the <see cref="IScheduler" /> that upon a mis-fire
        /// situation, the <see cref="SimpleTrigger" /> wants to be
        /// re-scheduled to the next scheduled time after 'now' - taking into
        /// account any associated <see cref="ICalendar" />, and with the
        /// repeat count left unchanged.
        /// <p>
        /// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
        /// the repeat-count that it was originally setup with (this is only an
        /// issue if you for some reason wanted to be able to tell what the original
        /// values were at some later time).
        /// </p>
        /// <p>
        /// <i>NOTE/WARNING:</i> This instruction could cause the <see cref="Trigger" />
        /// to go directly to the 'COMPLETE' state if all fire-times where missed.
        /// </p>
        /// </summary>
        public const int RescheduleNextWithExistingCount = 5;
    }
}
