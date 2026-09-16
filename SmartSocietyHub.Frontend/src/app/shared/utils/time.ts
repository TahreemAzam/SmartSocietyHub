// Backend TimeSpan fields (Facility opening/closing, Booking start/end)
// serialize as "HH:mm:ss". <input type="time"> needs "HH:mm".

export function toTimeInputValue(apiTime: string | undefined | null): string {
  return apiTime ? apiTime.slice(0, 5) : '';
}

export function toApiTimeValue(inputTime: string): string {
  return inputTime ? `${inputTime}:00` : '';
}

export function formatTimeLabel(apiTime: string | undefined | null): string {
  if (!apiTime) {
    return '';
  }

  const [hoursStr, minutesStr] = apiTime.split(':');
  const hours = Number(hoursStr);
  const minutes = Number(minutesStr);
  const period = hours >= 12 ? 'PM' : 'AM';
  const displayHours = hours % 12 === 0 ? 12 : hours % 12;

  return `${displayHours}:${minutesStr.padStart(2, '0')} ${period}`;
}
