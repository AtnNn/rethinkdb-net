using RethinkDb.Spec;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class TableQuery<T> : ISequenceQuery<T>
    {
        private readonly DbQuery dbTerm;
        private readonly string table;
        private readonly bool useOutdated;

        public TableQuery(DbQuery dbTerm, string table, bool useOutdated)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.useOutdated = useOutdated;
        }

        public InsertQuery<T> Insert(T @object, bool upsert = false)
        {
            return new InsertQuery<T>(this, new T[] { @object }, upsert);
        }

        public InsertQuery<T> Insert(IEnumerable<T> @objects, bool upsert = false)
        {
            return new InsertQuery<T>(this, @objects, upsert);
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm(datumConverterFactory));
            tableTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = table,
                    }
                }
            );
            if (useOutdated)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "use_outdated",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = useOutdated,
                        }
                    }
                });
            }
            return tableTerm;
        }
    }
}