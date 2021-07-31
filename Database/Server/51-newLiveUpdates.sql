-- Fix the members window not getting refreshed when a member is updated in an alliance
REPLACE INTO `eveliveupdates` (`updateID`, `updateName`, `description`, `machoVersionMin`, `machoVersionMax`, `buildNumberMin`, `buildNumberMax`, `methodName`, `objectID`, `codeType`, `code`) VALUES (9, 'allianceWindows', 'Fixes a bug with the update of the information in the alliance window', 219, 219, 101786, 101786, 'LoadViewClass', 'form.Alliances::FormAlliances', 'globalClassMethod', 0x630200000002000000020000004300000073060200007400006901007C0000690200690300830100017C01006401006A02006F1F00017404006905007C00006902006903008301007C00006902005F06006EA201017C01006402006A02006F1F00017404006907007C00006902006903008301007C00006902005F06006E7601017C01006403006A02006F1F00017404006908007C00006902006903008301007C00006902005F06006E4A01017C01006404006A02006F1F00017404006909007C00006902006903008301007C00006902005F06006E1E01017C01006405006A02006F080001640000536E0901017C01006406006A02006F350001740400690A007C00006902006903008301007C00006902005F06007C0000690200690600690B00740C00690D00830100016EC700017C01006407006A02006F350001740400690A007C00006902006903008301007C00006902005F06007C0000690200690600690B00740C00690E00830100016E8500017C01006408006A02006F350001740400690A007C00006902006903008301007C00006902005F06007C0000690200690600690B00740C00690F00830100016E4300017C01006409006A02006F350001740400690A007C00006902006903008301007C00006902005F06007C0000690200690600690B00740C00691000830100016E0100017C0000690200690600691100830000017400006912007C00006902006906005F130064000053280A0000004E740E000000616C6C69616E6365735F686F6D657412000000616C6C69616E6365735F72616E6B696E67737416000000616C6C69616E6365735F6170706C69636174696F6E737411000000616C6C69616E6365735F6D656D626572737417000000616C6C69616E6365735F72656C6174696F6E7368697073741100000072656C6174696F6E73686970735F6E6170741500000072656C6174696F6E73686970735F667269656E6473741900000072656C6174696F6E73686970735F636F6D70657469746F7273741500000072656C6174696F6E73686970735F656E656D696573281400000074030000007569787405000000466C75736874020000007372740D000000776E6456696577506172656E747404000000666F726D740D000000416C6C69616E636573486F6D65740B00000063757272656E74566965777411000000416C6C69616E63657352616E6B696E67737415000000416C6C69616E6365734170706C69636174696F6E737410000000416C6C69616E6365734D656D626572737416000000416C6C69616E63657352656C6174696F6E73686970737407000000536574547970657405000000636F6E73747417000000616C6C69616E636552656C6174696F6E736869704E4150741A000000616C6C69616E636552656C6174696F6E73686970467269656E64741E000000616C6C69616E636552656C6174696F6E73686970436F6D70657469746F727419000000616C6C69616E636552656C6174696F6E73686970456E656D79740C00000043726561746557696E646F77740900000055495F4E4F524D414C740500000073746174652802000000740400000073656C6674070000007461624E616D652800000000280000000073090000003C636F6E736F6C653E740D0000004C6F616456696577436C617373010000007332000000000113010D011F010D011F010D011F010D011F010D0108010D011B011A010D011B011A010D011B011A010D011B011A011001);
