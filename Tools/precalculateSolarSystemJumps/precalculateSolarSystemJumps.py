#
# Script adapted from the example on the beautiful book "EVE Market Strategies"
# written by Orbital Industries
# https://orbitalenterprises.github.io/eve-market-strategies/index.html#example-3---trading-rules-build-a-buy-matching-algorithm
#

from scipy.sparse import csr_matrix
from scipy.sparse.csgraph import shortest_path
import pymysql.cursors
import math

solar_list = []
solar_map = {}

connection = pymysql.connect(
    host='localhost',
    user='user',
    password='password',
    database='evesharp',
    cursorclass=pymysql.cursors.SSDictCursor)

with connection:
    with connection.cursor() as cursor:
        cursor.execute('CREATE TABLE IF NOT EXISTS `mapPrecalculatedSolarSystemJumps` (' \
                       '   `fromSolarSystemID` int(11) NOT NULL,' \
                       '   `toSolarSystemID` int(11) NOT NULL,' \
                       '   `jumps` int(11) DEFAULT NULL,'
                       '   PRIMARY KEY (`fromSolarSystemID`,`toSolarSystemID`)' \
                       ' ) ENGINE=InnoDB DEFAULT CHARSET=utf8')

    with connection.cursor() as cursor:
        # get the solar systems
        cursor.execute('SELECT solarSystemID FROM mapSolarSystems')
        try:
            while True:
                entry = cursor.fetchone()
                solar_list.append(entry['solarSystemID'])
        except:
            pass

    for next_solar in solar_list:
        solar_map[next_solar] = [next_solar]

    with connection.cursor() as cursor:
        # get jumps for this solar system
        cursor.execute('SELECT fromSolarSystemID, toSolarSystemID FROM mapSolarSystemJumps')
        try:
            while True:
                entry = cursor.fetchone ()
                if entry['toSolarSystemID'] not in solar_map[entry['fromSolarSystemID']]:
                    solar_map[entry['fromSolarSystemID']].append(entry['toSolarSystemID'])
        except:
            pass


    # link lists should be ready now
    # time to use scipy for the calculations
    solar_count = len(solar_list)
    adj_array = []
    for i in range(solar_count):
        next_row = []
        source_solar = solar_list[i]

        for j in range(solar_count):
            dest_solar = solar_list[j]

            if dest_solar in solar_map[source_solar]:
                next_row.append(1)
            else:
                next_row.append(0)

        adj_array.append(next_row)

    adj_matrix = csr_matrix(adj_array)

    shortest_matrix = shortest_path(adj_matrix, directed = False, return_predecessors = False, unweighted = True)

    # we should now have shortest paths available to be calculated
    # iterate the full list and add the proper records to the database
    for i in range(solar_count):
        source_solar = solar_list[i]

        for j in range(solar_count):
            dest_solar = solar_list[j]

            # create the record in the database
            with connection.cursor() as cursor:
                jumps = shortest_matrix[i][j]

                # ignore unreachable solar systems
                if jumps == math.inf:
                    continue

                cursor.execute('INSERT INTO `mapPrecalculatedSolarSystemJumps`(fromSolarSystemID, toSolarSystemID, jumps)VALUES(%s, %s, %s)', (source_solar, dest_solar, jumps))


    # comit all the changes into the database
    connection.commit()
