tapimedialog
Утилита для отправки TAPI-событий в программу Медиалог через буферный файл Medialog\PRIVATE\CISCO_CALL.INI
Подробности в medialog_call-center.txt

Для доступа к TAPI нужно запускать утилиту с правами локального администратора, как это обойти неизвестно.
Для работы нужно наличие в каталоге утилиты файла-конфига config.xml
параметры:

tapi_line_name 
линия на которой нужно регистрироваться, 
если не указано, будет пытаться зарегистрироваться на всех линиях, на которых доступно TAPIMEDIATYPE_AUDIO

outputfile
полный путь к буферному файлу вида
C:\Program Files\pmt\medialog7\private\CISCO_CALL.INI

debug_level
уровень детализации сообщений
NONE = 0,
LOW = 1,
MEDIUM = 2,
HIGH = 3,
DEBUG = 4,
DEVEL = 5

mappings
названия полей, которые должны писаться в буферный файл, они же названия столбцов в БД Медиалога
		<CALLERIDNAME>		имя звонящего
		<CALLERIDNUMBER>	номер звонящего
		<LineName>			название линии TAPI
		<CALLEDIDNAME>		имя вызывающего
		<CALLEDIDNUMBER>	номер вызывающего
		<CALLID>			ID звонка, в данный момент это внутренний хэш (A hash code for the current System.Object)
		<CallDirection>		направление звонка, Incoming, Outgoing.

Тестировалось с TAPI-провайдером Activa for Asterisk http://sourceforge.net/projects/activa/

Полезные ссылки
http://www.tapi.info

etCallerID Sample Program
http://www.exceletel.com/products/TeleTools/SamplePrograms/etCallerID/index.htm

estos tapi capability browser, 
ESTOS Phone Dialer
http://www.estos.de/support/download/support-tools.html
http://www.estos.de/uploads/tx_abdownloads/files/EPhone.zip


http://itapi3.codeplex.com
http://www.codeproject.com/Articles/10994/TAPI-3-0-Application-development-using-C-NET