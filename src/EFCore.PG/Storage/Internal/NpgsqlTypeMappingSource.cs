#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlTypeMappingSource : RelationalTypeMappingSource
    {
        readonly ConcurrentDictionary<string, RelationalTypeMapping[]> _storeTypeMappings;
        readonly ConcurrentDictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        #region Mappings

        readonly NpgsqlBoolTypeMapping         _bool           = new NpgsqlBoolTypeMapping();
        readonly NpgsqlByteArrayTypeMapping    _bytea          = new NpgsqlByteArrayTypeMapping();
        readonly FloatTypeMapping              _float4         = new FloatTypeMapping("real", DbType.Single);
        readonly DoubleTypeMapping             _float8         = new DoubleTypeMapping("double precision", DbType.Double);
        readonly DecimalTypeMapping            _numeric        = new DecimalTypeMapping("numeric", DbType.Decimal);
        readonly DecimalTypeMapping            _money          = new DecimalTypeMapping("money");
        readonly GuidTypeMapping               _uuid           = new GuidTypeMapping("uuid", DbType.Guid);
        readonly ShortTypeMapping              _int2           = new ShortTypeMapping("smallint", DbType.Int16);
        readonly ByteTypeMapping               _int2Byte       = new ByteTypeMapping("smallint", DbType.Byte);
        readonly IntTypeMapping                _int4           = new IntTypeMapping("integer", DbType.Int32);
        readonly LongTypeMapping               _int8           = new LongTypeMapping("bigint", DbType.Int64);
        readonly StringTypeMapping             _text           = new StringTypeMapping("text", DbType.String);
        readonly StringTypeMapping             _varchar        = new StringTypeMapping("character varying", DbType.String);
        readonly StringTypeMapping             _char           = new StringTypeMapping("character", DbType.String);
        readonly NpgsqlJsonbTypeMapping        _jsonb          = new NpgsqlJsonbTypeMapping();
        readonly NpgsqlJsonTypeMapping         _json           = new NpgsqlJsonTypeMapping();
        readonly NpgsqlXmlTypeMapping          _xml            = new NpgsqlXmlTypeMapping();
        readonly NpgsqlCitextTypeMapping       _citext         = new NpgsqlCitextTypeMapping();
        readonly NpgsqlDateTypeMapping         _date           = new NpgsqlDateTypeMapping();
        readonly NpgsqlTimestampTypeMapping    _timestamp      = new NpgsqlTimestampTypeMapping();
        readonly NpgsqlTimestampTzTypeMapping  _timestamptz    = new NpgsqlTimestampTzTypeMapping(typeof(DateTime));
        readonly NpgsqlTimestampTzTypeMapping  _timestamptzDto = new NpgsqlTimestampTzTypeMapping(typeof(DateTimeOffset));
        readonly NpgsqlIntervalTypeMapping     _interval       = new NpgsqlIntervalTypeMapping();
        readonly NpgsqlTimeTypeMapping         _time           = new NpgsqlTimeTypeMapping();
        readonly NpgsqlTimeTzTypeMapping       _timetz         = new NpgsqlTimeTzTypeMapping();
        readonly NpgsqlMacaddrTypeMapping      _macaddr        = new NpgsqlMacaddrTypeMapping();
        readonly NpgsqlMacaddr8TypeMapping     _macaddr8       = new NpgsqlMacaddr8TypeMapping();
        readonly NpgsqlInetTypeMapping         _inet           = new NpgsqlInetTypeMapping();
        readonly NpgsqlCidrTypeMapping         _cidr           = new NpgsqlCidrTypeMapping();
        readonly NpgsqlBitTypeMapping          _bit            = new NpgsqlBitTypeMapping();
        readonly NpgsqlVarbitTypeMapping       _varbit         = new NpgsqlVarbitTypeMapping();
        readonly NpgsqlHstoreTypeMapping       _hstore         = new NpgsqlHstoreTypeMapping();
        readonly NpgsqlPointTypeMapping        _point          = new NpgsqlPointTypeMapping();
        readonly NpgsqlBoxTypeMapping          _box            = new NpgsqlBoxTypeMapping();
        readonly NpgsqlLineTypeMapping         _line           = new NpgsqlLineTypeMapping();
        readonly NpgsqlLineSegmentTypeMapping  _lseg           = new NpgsqlLineSegmentTypeMapping();
        readonly NpgsqlPathTypeMapping         _path           = new NpgsqlPathTypeMapping();
        readonly NpgsqlPolygonTypeMapping      _polygon        = new NpgsqlPolygonTypeMapping();
        readonly NpgsqlCircleTypeMapping       _circle         = new NpgsqlCircleTypeMapping();
        readonly NpgsqlXidTypeMapping          _xid            = new NpgsqlXidTypeMapping();
        readonly NpgsqlOidTypeMapping          _oid            = new NpgsqlOidTypeMapping();
        readonly NpgsqlCidTypeMapping          _cid            = new NpgsqlCidTypeMapping();
        readonly NpgsqlRegtypeTypeMapping      _regtype        = new NpgsqlRegtypeTypeMapping();

        // Full text search mappings
        readonly NpgsqlTsQueryTypeMapping   _tsquery           = new NpgsqlTsQueryTypeMapping();
        readonly NpgsqlTsVectorTypeMapping  _tsvector          = new NpgsqlTsVectorTypeMapping();
        readonly NpgsqlTsRankingNormalizationTypeMapping _rankingNormalization = new NpgsqlTsRankingNormalizationTypeMapping();

        // Range mappings
        readonly NpgsqlRangeTypeMapping<int>      _int4range;
        readonly NpgsqlRangeTypeMapping<long>     _int8range;
        readonly NpgsqlRangeTypeMapping<decimal>  _numrange;
        readonly NpgsqlRangeTypeMapping<DateTime> _tsrange;
        readonly NpgsqlRangeTypeMapping<DateTime> _tstzrange;
        readonly NpgsqlRangeTypeMapping<DateTime> _daterange;

        #endregion Mappings

        public NpgsqlTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            // Initialize some mappings which depend on other mappings
            _int4range = new NpgsqlRangeTypeMapping<int>("int4range", typeof(NpgsqlRange<int>), _int4, NpgsqlDbType.Integer);
            _int8range = new NpgsqlRangeTypeMapping<long>("int8range", typeof(NpgsqlRange<long>), _int8, NpgsqlDbType.Bigint);
            _numrange  = new NpgsqlRangeTypeMapping<decimal>("numrange",  typeof(NpgsqlRange<decimal>), _numeric, NpgsqlDbType.Numeric);
            _tsrange   = new NpgsqlRangeTypeMapping<DateTime>("tsrange", typeof(NpgsqlRange<DateTime>), _timestamp, NpgsqlDbType.Range | NpgsqlDbType.Timestamp);
            _tstzrange = new NpgsqlRangeTypeMapping<DateTime>("tstzrange", typeof(NpgsqlRange<DateTime>), _timestamptz, NpgsqlDbType.Range | NpgsqlDbType.TimestampTZ);
            _daterange = new NpgsqlRangeTypeMapping<DateTime>("daterange", typeof(NpgsqlRange<DateTime>), _timestamptz, NpgsqlDbType.Range | NpgsqlDbType.Date);

            // Note that PostgreSQL has aliases to some built-in type name aliases (e.g. int4 for integer),
            // these are mapped as well.
            // https://www.postgresql.org/docs/current/static/datatype.html#DATATYPE-TABLE
            var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "boolean",                     new[] { _bool                         } },
                { "bool",                        new[] { _bool                         } },
                { "bytea",                       new[] { _bytea                        } },
                { "real",                        new[] { _float4                       } },
                { "float4",                      new[] { _float4                       } },
                { "double precision",            new[] { _float8                       } },
                { "float8",                      new[] { _float8                       } },
                { "numeric",                     new[] { _numeric                      } },
                { "decimal",                     new[] { _numeric                      } },
                { "money",                       new[] { _money                        } },
                { "uuid",                        new[] { _uuid                         } },
                { "smallint",                    new RelationalTypeMapping[] { _int2, _int2Byte } },
                { "int2",                        new RelationalTypeMapping[] { _int2, _int2Byte } },
                { "integer",                     new[] { _int4                         } },
                { "int",                         new[] { _int4                         } },
                { "int4",                        new[] { _int4                         } },
                { "bigint",                      new[] { _int8                         } },
                { "int8",                        new[] { _int8                         } },
                { "text",                        new[] { _text                         } },
                { "jsonb",                       new[] { _jsonb                        } },
                { "json",                        new[] { _json                         } },
                { "xml",                         new[] { _xml                          } },
                { "citext",                      new[] { _citext                       } },
                { "character varying",           new[] { _varchar                      } },
                { "varchar",                     new[] { _varchar                      } },
                { "character",                   new[] { _char                         } },
                { "char",                        new[] { _char                         } },
                { "date",                        new[] { _date                         } },
                { "timestamp without time zone", new[] { _timestamp                    } },
                { "timestamp",                   new[] { _timestamp                    } },
                { "timestamp with time zone",    new[] { _timestamptz, _timestamptzDto } },
                { "timestamptz",                 new[] { _timestamptz, _timestamptzDto } },
                { "interval",                    new[] { _interval                     } },
                { "time without time zone",      new[] { _time                         } },
                { "time",                        new[] { _time                         } },
                { "time with time zone",         new[] { _timetz                       } },
                { "timetz",                      new[] { _timetz                       } },
                { "macaddr",                     new[] { _macaddr                      } },
                { "macaddr8",                    new[] { _macaddr8                     } },
                { "inet",                        new[] { _inet                         } },
                { "cidr",                        new[] { _cidr                         } },
                { "bit",                         new[] { _bit                          } },
                { "bit varying",                 new[] { _varbit                       } },
                { "varbit",                      new[] { _varbit                       } },
                { "hstore",                      new[] { _hstore                       } },
                { "point",                       new[] { _point                        } },
                { "box",                         new[] { _box                          } },
                { "line",                        new[] { _line                         } },
                { "lseg",                        new[] { _lseg                         } },
                { "path",                        new[] { _path                         } },
                { "polygon",                     new[] { _polygon                      } },
                { "circle",                      new[] { _circle                       } },
                { "xid",                         new[] { _xid                          } },
                { "oid",                         new[] { _oid                          } },
                { "cid",                         new[] { _cid                          } },
                { "regtype",                     new[] { _regtype                      } },

                { "int4range",                   new[] { _int4range                    } },
                { "int8range",                   new[] { _int8range                    } },
                { "numrange",                    new[] { _numrange                     } },
                { "tsrange",                     new[] { _tsrange                      } },
                { "tstzrange",                   new[] { _tstzrange                    } },
                { "daterange",                   new[] { _daterange                    } },

                { "tsquery",                     new[] { _tsquery                      } },
                { "tsvector",                    new[] { _tsvector                     } }
            };

            var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(bool),                       _bool           },
                { typeof(byte[]),                     _bytea          },
                { typeof(float),                      _float4         },
                { typeof(double),                     _float8         },
                { typeof(decimal),                    _numeric        },
                { typeof(Guid),                       _uuid           },
                { typeof(byte),                       _int2Byte       },
                { typeof(short),                      _int2           },
                { typeof(int),                        _int4           },
                { typeof(long),                       _int8           },
                { typeof(string),                     _text           },
                { typeof(DateTime),                   _timestamp      },
                { typeof(TimeSpan),                   _interval       },
                { typeof(DateTimeOffset),             _timestamptzDto },
                { typeof(PhysicalAddress),            _macaddr        },
                { typeof(IPAddress),                  _inet           },
                { typeof(BitArray),                   _varbit         },
                { typeof(Dictionary<string, string>), _hstore         },
                { typeof(NpgsqlPoint),                _point          },
                { typeof(NpgsqlBox),                  _box            },
                { typeof(NpgsqlLine),                 _line           },
                { typeof(NpgsqlLSeg),                 _lseg           },
                { typeof(NpgsqlPath),                 _path           },
                { typeof(NpgsqlPolygon),              _polygon        },
                { typeof(NpgsqlCircle),               _circle         },

                { typeof(NpgsqlRange<int>),           _int4range      },
                { typeof(NpgsqlRange<long>),          _int8range      },
                { typeof(NpgsqlRange<decimal>),       _numrange       },
                { typeof(NpgsqlRange<DateTime>),      _tsrange        },

                { typeof(NpgsqlTsQuery),              _tsquery        },
                { typeof(NpgsqlTsVector),             _tsvector       },
                { typeof(NpgsqlTsRankingNormalization), _rankingNormalization }
            };

            _storeTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
            _clrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

            if (typeof(NpgsqlConnection).Assembly.GetName().Version < new Version(3, 3))
                SetupLegacyPostgisMappings();
        }

        void SetupLegacyPostgisMappings()
        {
            // This mapping is only used in Npgsql 3.2 and below.
            // Later versions use type plugins to set up mappings, and corresponding EF Core
            // plugins need to be used.
            var legacyPostgisMapping = new NpgsqlLegacyPostgisTypeMapping();
            _storeTypeMappings["geometry"] = new[] { legacyPostgisMapping };
            _clrTypeMappings[typeof(PostgisGeometry)] = legacyPostgisMapping;
        }

        protected override RelationalTypeMapping FindMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mappings))
                {
                    if (clrType == null)
                        return mappings[0];

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m;

                    return null;
                }

                if (_storeTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
                {
                    if (clrType == null)
                        return mappings[0].Clone(mappingInfo);

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m.Clone(mappingInfo);

                    return null;
                }

                return FindArrayMapping(mappingInfo);
            }

            if (clrType == null)
                return null;

            if (!_clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                // TODO: range, enum, composite

                // No mapping found which corresponds to the clrType, try to find an array
                return FindArrayMapping(mappingInfo);
            }

            // If needed, clone the mapping with the configured length/precision/scale
            // TODO: Cache size/precision/scale mappings?
            if (mappingInfo.Size.HasValue)
            {
                if (clrType == typeof(string))
                {
                    // See #342 for when size > 10485760
                    return mappingInfo.Size <= 10485760
                        ? _varchar.Clone($"varchar({mappingInfo.Size})", mappingInfo.Size)
                        : _text;
                }

                if (clrType == typeof(BitArray))
                    return _varbit.Clone($"varbit({mappingInfo.Size})", mappingInfo.Size);
            }
            else if (mappingInfo.Precision.HasValue)
            {
                if (clrType == typeof(decimal))
                {
                    return _numeric.Clone(mappingInfo.Scale.HasValue
                        ? $"numeric({mappingInfo.Precision.Value},{mappingInfo.Scale.Value})"
                        : $"numeric({mappingInfo.Precision.Value})",
                        null);
                }

                if (clrType == typeof(DateTime) ||
                    clrType == typeof(DateTimeOffset) ||
                    clrType == typeof(TimeSpan))
                {
                    return mapping.Clone($"{mapping.StoreType}({mappingInfo.Precision.Value})", null);
                }
            }

            return mapping;
        }

        RelationalTypeMapping FindArrayMapping(RelationalTypeMappingInfo mappingInfo)
        {
            // PostgreSQL array type names are the element plus []
            var storeType = mappingInfo.StoreTypeName;
            if (storeType != null && storeType.EndsWith("[]"))
            {
                // TODO: In theory support the multiple mappings just like we do with scalars above
                // (e.g. DateTimeOffset[] vs. DateTime[]
                var elementMapping = FindMapping(storeType.Substring(0, storeType.Length - 2));
                if (elementMapping != null)
                    return _storeTypeMappings.GetOrAdd(storeType,
                        new RelationalTypeMapping[] { new NpgsqlArrayTypeMapping(storeType, elementMapping) })[0];
            }

            var clrType = mappingInfo.ClrType;
            if (clrType == null)
                return null;

            // Try to see if it is an array type
            var arrayElementType = GetArrayElementType(clrType);
            if (arrayElementType != null)
            {
                var elementMapping = (RelationalTypeMapping)FindMapping(arrayElementType);

                // If an element isn't supported, neither is its array
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return _clrTypeMappings.GetOrAdd(clrType, new NpgsqlArrayTypeMapping(elementMapping, clrType));
            }

            return null;
        }

        [CanBeNull]
        static Type GetArrayElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return type.GetElementType();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            return ilist != null ? ilist.GetGenericArguments()[0] : null;
        }
    }
}
